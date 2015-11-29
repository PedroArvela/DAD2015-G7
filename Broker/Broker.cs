using SESDADLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;

namespace Broker {
    public class Broker : Node, INode {
        private bool _routingPolicy;
        private Queue<Message> _queue = new Queue<Message>();
        private Object _queueLock = new Object();

        // Key: Topic, Value: Nodes
        private Dictionary<string, Dictionary<string, INode>> topicSubscribers = new Dictionary<string, Dictionary<string, INode>>();

        // Key: URL, Value: Node
        private Dictionary<string, INode> subscribers = new Dictionary<string, INode>();
        private Dictionary<string, INode> children = new Dictionary<string, INode>();
        private Tuple<string, INode> parent;

        private bool _delayed = false;
        private int _delayTime = 0;

        public Broker(string processName, string processURL, string site, string routingtype, string puppetMasterURL, string loggingLevel) : base(processName, processURL, site, puppetMasterURL) {
            _puppetMasterURL = puppetMasterURL;
            switch (routingtype) {
                case "flooding":
                    _routingPolicy = false;
                    break;
                case "filter":
                    _routingPolicy = true;
                    break;
            }
            _loggingLevel = loggingLevel;

            //process start arguments
            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Broker\\bin\\Debug\\Broker.exe";
        }

        public string getParentURL() {
            if (parent != null) {
                return parent.Item1;
            } else {
                return "";
            }
        }

        public void addSubscriberUrl(string url) {
            INode node = aquireConnection(url);
            subscribers.Add(url, node);
        }

        public void addChildUrl(string url) {
            INode node = aquireConnection(url);
            children.Add(url, node);
        }
        public void addParentUrl(string url) {
            INode node = aquireConnection(url);
            parent = new Tuple<string, INode>(url, node);
        }

        public bool toggleNode() {
            return (_enabled = !_enabled);
        }

        public bool toggleDelay(int time) {
            _delayTime = time;
            return (_delayed = !_delayed);
        }

        public void setRoutingPolicy(bool policy) { _routingPolicy = policy; }

        public override string showNode() {
            string print = "\tBroker: " + _processName + " for " + _site + " active on " + _processURL + "\n";
            if (_routingPolicy) {
                print += "\tRouting Policy: filter\n";
            } else {
                print += "\tRouting Policy: flooding\n";
            }
            print += "\tParent Broker URL(s):\n";
            if (parent != null) {
                print += "\t\t" + parent.Item1 + "\n";
            }

            print += "\tChild Broker URL(s):\n";
            foreach (string curl in children.Keys) {
                print += "\t\t" + curl + "\n";
            }
            print += "\tTopic(s):\n";
            foreach (string topic in topicSubscribers.Keys) {
                print += "\t\t" + topic + " has the following interested urls:\n";
                foreach (string url in topicSubscribers[topic].Keys) {
                    print += "\t\t\t" + url + "\n";
                }
            }
            print += "\tSubscriber(s):\n";
            foreach (string subscriber in subscribers.Keys) {
                print += "\t\t" + subscriber + "\n";
            }
            return print;
        }

        private void removeTopic(string topic, string interestedURL) {
            Console.WriteLine("Removing from interest list...");

            if (!topicSubscribers.ContainsKey(topic)) {
                Console.WriteLine("No topic matching " + topic + " found");
                return;
            }

            topicSubscribers[topic].Remove(interestedURL);
            Console.WriteLine("Removed from topic " + topic + " intereste url " + interestedURL);

            if (topicSubscribers[topic].Count == 0) {
                Console.WriteLine("Topic no longer has any interested url");
                topicSubscribers.Remove(topic);
            }
        }

        public void addTopic(string topic, string interestedURL) {
            bool found = false;
            INode node;

            // Get the interested node from the URL
            if (parent != null && interestedURL == parent.Item1) {
                node = parent.Item2;
            } else if (subscribers.ContainsKey(interestedURL)) {
                node = subscribers[interestedURL];
            } else {
                node = children[interestedURL];
            }

            Console.WriteLine("Adding to interest List...");
            if (!topicSubscribers.ContainsKey(topic)) {

                Console.WriteLine("New topic " + interestedURL + " added with interested url " + interestedURL);
                topicSubscribers.Add(topic, new Dictionary<string, INode>());
            } else {
                foreach (var sub in topicSubscribers[topic]) {
                    if (interestedURL.Equals(sub.Key)) {
                        Console.WriteLine("URL already on interest list");
                        found = true;
                        break;
                    }
                }
            }

            if (!found) {
                Console.WriteLine("URL: " + interestedURL + " added to topic " + topic);
                topicSubscribers[topic].Add(interestedURL, node);
            }
        }

        public void addToQueue(Message p) {
            lock (_queueLock) {
                _queue.Enqueue(p);
            }
        }

        public void sendPublication(Message pub) {
            List<INode> targets = new List<INode>();

            if (!_routingPolicy) {
                Console.WriteLine("Sending Message in Flood Mode...");

                if (parent != null && parent.Item1 != pub.originURL) {
                    targets.Add(parent.Item2);
                }

                foreach (var node in children) {
                    if (node.Key != pub.originURL) {
                        targets.Add(node.Value);
                    }
                }

                foreach (var node in subscribers) {
                    if (node.Key != pub.originURL) {
                        targets.Add(node.Value);
                    }
                }

            } else {
                Console.WriteLine("Sending message in Filter Mode...");

                foreach (string interestedTopic in topicSubscribers.Keys) {
                    Console.WriteLine("testing... " + interestedTopic + " matches " + pub.Topic);
                    if (this.compatibleTopics(interestedTopic, pub.Topic)) {
                        foreach (INode node in topicSubscribers[interestedTopic].Values) {
                            targets.Add(node);
                        }
                    }
                }
            }

            // Send the message to all the selected nodes
            pub.originURL = _processURL;
            foreach (INode node in targets) {
                node.addToQueue(pub);

                if (_loggingLevel.Equals("full"))
                    this.writeToLog("BroEvent " + _processName + ", " + pub.Publisher + ", " + pub.Topic + ", " + pub.Sequence);

                Console.WriteLine("Publication sent to: " + node.Url());
            }
        }

        private bool compatibleTopics(string topic, string test) {
            // This code scares me. 5 or more levels of chained ifs or loops
            // are not a good sign under any circumstances -- Arvela

            string[] masterTopic = topic.Split('/');
            string[] testTopic = test.Split('/');
            int masterTopicSize = masterTopic.Length;
            int testTopicSize = testTopic.Length;

            Console.Write("analysing MASTER:\n\t");
            for (int i = 0; i < masterTopicSize; i++) {
                Console.Write(masterTopic[i] + " ");
            }
            Console.Write("\nanalysing TEST:\n\t");
            for (int i = 0; i < testTopicSize; i++) {
                Console.Write(testTopic[i] + " ");
            }

            if (masterTopic[0].Equals("*") || topic.Equals(test)) {
                Console.Write("\nMatch\n");
                return true;
            } else {
                if (masterTopicSize == testTopicSize) {
                    for (int i = 0; i < testTopicSize; i++) {
                        if (!masterTopic[i].Equals(testTopic[i])) {
                            if (masterTopic[i].Equals("*") || testTopic[0].Equals("*")) {
                                Console.Write("\nMatch\n");
                                return true;
                            }
                            Console.Write("\nNO-Match\n");
                            return false;
                        }
                    }
                } else if (masterTopicSize > testTopicSize) {
                    Console.Write("\nNO-Match\n");
                    return false;
                } else if (masterTopicSize < testTopicSize) {
                    for (int i = 0; i < testTopicSize; i++) {
                        if (i >= masterTopicSize) {
                            Console.Write("\nNO-Match\n");
                            return false;
                        }
                        if (!masterTopic[i].Equals(testTopic[i])) {
                            if (masterTopic[i].Equals("*")) {
                                Console.Write("\nMatch\n");
                                return true;
                            }
                        }
                    }
                    Console.Write("\nNO-Match\n");
                    return false;
                }
            }
            Console.Write("\nMatch\n");
            return true;
        }

        private void shareSubRequest(string topic, string origin, Message request) {
            List<INode> shareList = new List<INode>();

            if (request.SubType.Equals(MessageType.Subscribe)) {
                INode node;
                if (parent != null && parent.Item1 == origin) {
                    node = parent.Item2;
                } else if (children.Keys.Contains(origin)) {
                    node = children[origin];
                } else {
                    node = subscribers[origin];
                }

                if (!topicSubscribers.ContainsKey(topic)) {
                    topicSubscribers.Add(topic, new Dictionary<string, INode>());
                }

                if (topicSubscribers[topic].ContainsKey(origin)) {
                    Console.WriteLine(origin + " is already flaged as interested on " + topic);
                    return;
                }

                topicSubscribers[topic].Add(origin, node);
            } else if (request.SubType.Equals(MessageType.Unsubscribe)) {
                if (!topicSubscribers.ContainsKey(topic)) {
                    Console.WriteLine("Nonexistent Topic, cannont unsubscribe...");
                    return;
                }

                topicSubscribers[topic].Remove(topic);

                if (topicSubscribers[topic].Count == 0) {
                    topicSubscribers.Remove(topic);
                }
            }

            //share request
            if (parent != null && parent.Item1 != origin) {
                shareList.Add(parent.Item2);
            }
            foreach (var node in children) {
                if (node.Key != origin) {
                    shareList.Add(node.Value);
                }
            }

            request.originURL = _processURL;
            foreach (INode node in shareList) {
                node.addToQueue(request);
            }
        }

        public void processQueue() {
            Message pub = null;
            if (_queue.Count > 0 && _enabled) {
                lock (_queueLock) {
                    pub = _queue.Dequeue();
                    Console.WriteLine("Processing: " + pub.ToString());
                    if (pub.SubType.Equals(MessageType.Subscribe) || pub.SubType.Equals(MessageType.Unsubscribe)) {
                        Console.WriteLine(pub.SubType.ToString() + " request for topic " + pub.Topic);
                        this.shareSubRequest(pub.Topic, pub.originURL, pub);
                    } else {
                        this.sendPublication(pub);
                    }
                }
            } else {
                //nothing to do
            }
        }

        public override void publishToPuppetMaster() {
            int port = Int32.Parse(_processURL.Split(':')[2].Split('/')[0]);
            string uri = _processURL.Split(':')[2].Split('/')[1];

            Console.WriteLine("Publishing on port: " + port.ToString() + " with uri: " + uri);

            TcpChannel channel = new TcpChannel(port);
            //Channel is already registered by PuppetMaster
            //ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, uri, typeof(Broker));
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        protected override string getArguments() {
            //processName processURL site routingtype puppetMasterURL loggingLevel -p parentURL -c childURL -s subURL
            string arguments = _processName + " " + _processURL + " " + _site + " ";
            if (_routingPolicy) {
                arguments += "filter" + " " + _puppetMasterURL + " " + _loggingLevel;
            } else {
                arguments += "flooding" + " " + _puppetMasterURL + " " + _loggingLevel;
            }

            if (parent != null) {
                arguments += " -p " + parent.Item1;
            }

            foreach (string child in children.Keys) {
                arguments += " -c " + child;
            }
            foreach (string sub in subscribers.Keys) {
                arguments += " -s " + sub;
            }
            return arguments;
        }

        public override void executeProcess() {
            _nodeProcess.StartInfo.Arguments = this.getArguments();
            _nodeProcess.Start();
            _executing = true;
        }
    }
}
