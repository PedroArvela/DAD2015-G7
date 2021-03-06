﻿using SESDADLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;

namespace Broker {
    public class Broker : Node, INode {
        private bool _routingPolicy;
        private string orderPolicy;
        private ConcurrentQueue<Message> sendQueue = new ConcurrentQueue<Message>();

        // Key: Topic, Value: Nodes
        private Dictionary<string, Dictionary<string, INode>> topicSubscribers = new Dictionary<string, Dictionary<string, INode>>();

        // Key: URL, Value: Node
        private Dictionary<string, INode> subscribers = new Dictionary<string, INode>();
        private Dictionary<string, INode> children = new Dictionary<string, INode>();
        private Dictionary<string, INode> parents = new Dictionary<string, INode>();
        private Tuple<string, INode> parent;

        private bool _delayed = false;
        private int _delayTime = 0;

        private bool _faultTolerant = false;
        private Dictionary<string, INode> backupBrokers = new Dictionary<string, INode>();

        // Total Message Ordering:
        // Publications from the parent which are ready to deliver with a proper order
        private Dictionary<int, Message> parentMessages = new Dictionary<int, Message>();
        private int lastParentIndex = -1;

        // Key: child URL, Value: last index sent to the child
        private Dictionary<string, int> childrenSendIndex = new Dictionary<string, int>();

        public Broker(string processName, string processURL, string site, string routingtype, string ordering, string puppetMasterURL, string loggingLevel, bool faultTolerant) : base(processName, processURL, site, puppetMasterURL) {
            _puppetMasterURL = puppetMasterURL;
            switch (routingtype) {
                case "flooding":
                    _routingPolicy = false;
                    break;
                case "filter":
                    _routingPolicy = true;
                    break;
            }
            this.orderPolicy = ordering;
            _loggingLevel = loggingLevel;
            _faultTolerant = faultTolerant;

            //process start arguments
            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Broker\\bin\\Debug\\Broker.exe";
        }

        public string getParentURL()
        {
            return parent.Item1;
        }

        public bool containsNode(string url) {
            return (parents.ContainsKey(url) || children.ContainsKey(url) || subscribers.ContainsKey(url));
        }

        public INode getNode(string url) {
            if (parents.ContainsKey(url)) {
                return parents[url];
            }

            if (children.ContainsKey(url)) {
                return children[url];
            }

            if (subscribers.ContainsKey(url)) {
                return subscribers[url];
            }

            throw new Exception("No such node exists");
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
            if(parent == null)
            {
                parent = new Tuple<string, INode>(url, node);
            }
            parents.Add(url, node);
        }
        public void addBackupUrl(string url)
        {
            INode node = aquireConnection(url);
            backupBrokers.Add(url, node);
        }

        public bool toggleNode() { return (_enabled = !_enabled); }
        public bool toggleDelay(int time) {
            _delayTime = time;
            return (_delayed = !_delayed);
        }

        public void setRoutingPolicy(bool policy) { _routingPolicy = policy; }
        public void setOrdering(string order) { orderPolicy = order; }

        // Process the queue of messages to send
        public void processQueue() {
            Message pub = null;
            HashSet<string> destinations;

            // If we can't run we should simply skip this part
            if (!_enabled || !sendQueue.TryDequeue(out pub)) {
                return;
            }
            
            if (pub.SubType.Equals(MessageType.Subscribe) || pub.SubType.Equals(MessageType.Unsubscribe)) {
                // If the message is a subscribe or unsubscribe event, just send it away
                Console.WriteLine(pub.SubType.ToString() + " request for topic " + pub.Topic);
                this.shareSubRequest(pub.Topic, pub.SenderSite, pub);
            } else {
                Console.WriteLine("Processing publication #" + pub.Sequence + " from " + pub.Publisher);
                // Otherwise run all of the tricky logic to send messages ordered
                lock (childrenSendIndex) {
                    destinations = getDestinations(pub);

                    Console.WriteLine("Sending " + pub.Topic + " to " + destinations.Count + " destinations");

                    // For each destination, assign a new message number and then send it
                    foreach (string destination in destinations) {
                        sendToDestination(pub, destination);
                    }
                }
            }

            if (_loggingLevel.Equals("full"))
                writeToLog("BroEvent " + _processName + ", " + pub.Publisher + ", " + pub.Topic + ", " + pub.Sequence);
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
            // (Re-)order message if necessary
            if (orderPolicy == "TOTAL" && p.SubType == MessageType.Publication && p.Ordered) {
                Console.WriteLine("Reordering ordered message from the parent");
                // check all backlogged ordered messages to send
                while (reorderPublication(p)) {
                    lock (parentMessages) {
                        if (parentMessages.Count == 0) {
                            break;
                        } else {
                            p = parentMessages[lastParentIndex + 1];
                            parentMessages.Remove(lastParentIndex);
                        }
                    }
                }
            } else if (orderPolicy == "TOTAL" && p.SubType == MessageType.Publication) {
                Console.WriteLine("Message with no order received.");
                orderPublication(p);
            } else {
                Console.WriteLine("Adding message to send queue");
                sendQueue.Enqueue(p);
            }
        }

        private void orderPublication(Message pub) {
            bool isParent = false;

            foreach (string topic in topicSubscribers.Keys) {
                // Check if any of the topics matches the parent
                if (parent == null ||
                    (topicSubscribers[topic].ContainsKey(parent.Item1) && !compatibleTopics(topic, pub.Topic))) {
                    isParent = true;
                }
            }

            if (!isParent) {
                // If we are not the parent, send up
                parent.Item2.addToQueue(pub);
            } else {
                pub.Ordered = true;
                pub.OrderingBroker = _processURL;
                sendQueue.Enqueue(pub);
            }
        }

        private bool reorderPublication(Message pub) {
            bool deliver = false;

            lock (parentMessages) {
                if (pub.Order == lastParentIndex + 1) {
                    // Send the message right away if it is in the correct order
                    lastParentIndex++;

                    deliver = true;
                } else {
                    // Wait for more messages to arrive
                    parentMessages.Add(pub.Order, pub);
                }
            }

            if (deliver == true) {
                lock (sendQueue) {
                    sendQueue.Enqueue(pub);
                }
            }

            return deliver;
        }

        // Send the message pub to the destination url
        private void sendToDestination(Message pub, string url) {
            if (!childrenSendIndex.ContainsKey(url)) {
                childrenSendIndex.Add(url, -1);
            }

            childrenSendIndex[url]++;
            pub.Order = childrenSendIndex[url];
            pub.Sender = _processURL;
            pub.SenderSite = getSite();

            getNode(url).addToQueue(pub);

            Console.WriteLine("Publication " + pub.Topic + " from " + pub.Publisher + " sent to: " + url);
        }

        // Returns the list of urls where the message should go to
        private HashSet<string> getDestinations(Message pub) {
            HashSet<string> destinations = new HashSet<string>();

            if (!_routingPolicy) {
                // Flood mode
                if (parent != null) {
                    destinations.Add(parent.Item1);
                }

                foreach (var node in children.Keys) {
                    destinations.Add(node);
                }

                foreach (var node in subscribers.Keys) {
                    destinations.Add(node);
                }
            } else {
                // Filter mode
                // Check if each topic is compatible and add its nodes to the list if so
                foreach (string topic in topicSubscribers.Keys) {
                    Console.WriteLine("Testing if " + topic + "matches" + pub.Topic);
                    if (compatibleTopics(topic, pub.Topic)) {
                        Console.WriteLine(topic + " matches " + pub.Topic + ". Adding " + topicSubscribers[topic].Count + " nodes");
                        foreach (string node in topicSubscribers[topic].Keys) {
                            destinations.Add(node);
                        }
                    }
                }
            }

            // Only send to the sender if we are the ones responsible for assigning it a number
            // Remove otherwise
            if (orderPolicy != "TOTAL" || pub.OrderingBroker != _processURL) {
                if (destinations.Contains(pub.Sender)) {
                    destinations.Remove(pub.Sender);
                }
            }

            return destinations;
        }

        // Give the message a suitable number which is different according to the node it was sent to but
        // still coherent among nodes.
        private void orderForNode(Message pub, string target) {
            string url = target;

            // This requires the parent function to lock the sendingQueue
            if (!childrenSendIndex.ContainsKey(url)) {
                childrenSendIndex.Add(url, -1);
            }

            childrenSendIndex[url]++;
            pub.Order = childrenSendIndex[url];
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
                INode node = null;
                if (parent != null && parent.Item2.getSite() == origin)
                    node = parent.Item2;
                foreach (INode n in children.Values)
                {
                    if (n.getSite() == origin)
                        node = n;
                }
                foreach (INode n in subscribers.Values)
                {
                    if (n.getSite() == origin)
                        node = n;
                }
                if (node == null)
                    return;

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

                topicSubscribers[topic].Remove(origin);

                if (topicSubscribers[topic].Count == 0) {
                    topicSubscribers.Remove(topic);
                }
            }

            //share request
            if (parent != null && parent.Item2.getSite() != origin) {
                shareList.Add(parent.Item2);
            }
            foreach (INode node in children.Values) {
                if (node.getSite() != origin) {
                    shareList.Add(node);
                }
            }

            request.Sender = _processURL;
            request.SenderSite = getSite();
            foreach (INode node in shareList) {
                node.addToQueue(request);
            }
        }

        public override void publishToPuppetMaster() {
            int port = Int32.Parse(_processURL.Split(':')[2].Split('/')[0]);
            string uri = _processURL.Split(':')[2].Split('/')[1];

            Console.WriteLine("Publishing on port: " + port.ToString() + " with uri: " + uri);

            TcpChannel channel = new TcpChannel(port);
            RemotingServices.Marshal(this, uri, typeof(Broker));
        }

        protected override string getArguments() {
            //processName processURL site routingtype puppetMasterURL loggingLevel faultTolerant -p parentURL -c childURL -s subURL -b backupURL
            string arguments = _processName + " " + _processURL + " " + _site + " ";
            if (_routingPolicy) {
                arguments += "filter";
            } else {
                arguments += "flooding";
            }

            arguments += " ";

            arguments += orderPolicy;

            arguments += " " + _puppetMasterURL + " " + _loggingLevel;

            arguments += " " + _faultTolerant;

            if (parent != null) {
                arguments += " -p " + parent.Item1;
            }

            foreach (string child in children.Keys) {
                arguments += " -c " + child;
            }
            foreach (string sub in subscribers.Keys) {
                arguments += " -s " + sub;
            }
            foreach(string backup in backupBrokers.Keys)
            {
                arguments += " -b " + backup;
            }
            return arguments;
        }

        public override void executeProcess() {
            _nodeProcess.StartInfo.Arguments = this.getArguments();
            _nodeProcess.Start();
            _executing = true;
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        public override string showNode() {
            string print = "\tBroker: " + _processName + " for " + _site + " active on " + _processURL + "\n";
            if (_routingPolicy) {
                print += "\tRouting Policy: filter\n";
            } else {
                print += "\tRouting Policy: flooding\n";
            }
            print += "\tParent Broker URL(s):\n";
            foreach (string curl in parents.Keys)
            {
                print += "\t\t" + curl + "\n";
            }

            print += "\tChild Broker URL(s):\n";
            foreach (string curl in children.Keys) {
                print += "\t\t" + curl + "\n";
            }
            if (_faultTolerant)
            {
                print += "\tBackup Brokers URL(s):\n";
                foreach(string bburl in backupBrokers.Keys)
                {
                    print += "\t\t" + bburl + "\n";
                }
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
    }
}
