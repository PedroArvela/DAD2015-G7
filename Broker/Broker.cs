using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using SESDADLib;
using Subscriber;


namespace Broker {
    public class Broker : Node, INode {
        private bool _routingPolicy;
        private Queue<Message> _queue;
        private Object _queueLock = new Object();

        private Dictionary<string, List<string>> _subscribersTopics = new Dictionary<string, List<string>>(); //key == topic, value == #interestedURL's
        private List<string> _subscribers = new List<string>();
        private List<string> _parentProcessesURL = new List<string>();
        private List<string> _childProcessesURL = new List<string>();

        private bool _delayed = false;
        private int _delayTime = 0;

        public Broker(string processName, string processURL, string site, string routingtype, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
            _puppetMasterURL = puppetMasterURL;
            _queue = new Queue<Message>();
            _subscribersTopics = new Dictionary<string, List<string>>();
            switch (routingtype) {
                case "flooding":
                    _routingPolicy = false;
                    break;
                case "filter":
                    _routingPolicy = true;
                    break;
            }

            //process start arguments
            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Broker\\bin\\Debug\\Broker.exe";
        }

        public List<string> getParentURL() { return _parentProcessesURL; }
        public List<string> getChildURL() { return _childProcessesURL; }
        public List<string> getSubURL() { return _subscribers; }

        public void addSubscriberUrl(string url) {
            _subscribers.Add(url);
        }

        public void addChildUrl(string url) {
            _childProcessesURL.Add(url);
        }
        public void addParentUrl(string url) {
            _parentProcessesURL.Add(url);
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
            print += "\tParent Broker URL(s):\n";
            foreach (string purl in _parentProcessesURL) {
                print += "\t\t" + purl + "\n";
            }
            print += "\tChild Broker URL(s):\n";
            foreach (string curl in _childProcessesURL) {
                print += "\t\t" + curl + "\n";
            }
            print += "\tTopic(s):\n";
            foreach (string topic in _subscribersTopics.Keys) {
                print += "\t\t" + topic + " has the following interested urls:\n";
                foreach(string url in _subscribersTopics[topic]) {
                    print += "\t\t\t" + url + "\n";
                }
            }
            print += "\tSubscriber(s):\n";
            foreach(string subscriber in _subscribers) {
                print += "\t\t" + subscriber + "\n";
            }
            return print;
        }

        private void removeTopic(string topic, string interestedURL) {
            Console.WriteLine("Removing from interest list...");

            if (_subscribersTopics[topic] == null) {
                Console.WriteLine("No topic matching " + topic + " found");
                return;
            } else {
                _subscribersTopics[topic].Remove(interestedURL);
                Console.WriteLine("Removed from topic " + topic + " intereste url " + interestedURL);
            }
            if (_subscribersTopics[topic].Count == 0) {
                Console.WriteLine("Topic no longer has any interested url");
                _subscribersTopics.Remove(topic);
            }
        }

        public void addTopic(string topic, string interestedURL) {
            List<string> interest = null;
            bool found = false;

            Console.WriteLine("Adding to interest List...");
            _subscribersTopics.TryGetValue(topic, out interest);
            if (interest == null ) {
                Console.WriteLine("New topic " + interestedURL + " added with interested url " + interestedURL);
                interest = new List<string>();
                interest.Add(interestedURL);
                _subscribersTopics.Add(topic, interest);
            } else {
                interest = _subscribersTopics[topic];
                foreach (string url in interest) {
                    if (interestedURL.Equals(url)) {
                        Console.WriteLine("URL already on interest list");
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    Console.WriteLine("URL: " + interestedURL + " added to topic " + topic);
                    interest.Add(interestedURL);
                }
            }
        }

        public void addToQueue(Message p) {
            lock (_queueLock) {
                _queue.Enqueue(p);
            }
        }

        public void sendPublication(Message pub) {
            INode target = null;
            Subscriber.Subscriber remoteS = null;
            List<string> targetURL = new List<string>();

            
            if (!_routingPolicy) {
                Console.WriteLine("Sending Message in Flood Mode...");
                foreach (string url in _childProcessesURL) {
                    if (!url.Equals(pub.originURL)) {
                        targetURL.Add(url);
                    }
                }
                foreach (string url in _parentProcessesURL) {
                    if (!url.Equals(pub.originURL)) {
                        targetURL.Add(url);
                    }
                }
                foreach (string url in _subscribers) {
                    if (!url.Equals(pub.originURL)) {
                        targetURL.Add(url);
                    }
                }

                //flood all available brokers
                pub.originURL = _processURL;
                foreach (string url in targetURL) {
                    target = (INode)Activator.GetObject(typeof(INode), url);
                    target.addToQueue(pub);
                    Console.WriteLine("Publication sent to: " + url);
                }

            } else {
                Console.WriteLine("Sending message in Filter Mode...");
                foreach (string interestedTopic in _subscribersTopics.Keys) {
                    Console.WriteLine("testing... " + interestedTopic + " matches " + pub.Topic);
                    if (interestedTopic.Equals(pub.Topic)) {
                        foreach (string interestedURL in _subscribersTopics[interestedTopic]) {
                            if (_subscribers.Contains(interestedURL)) {
                                remoteS = (Subscriber.Subscriber)Activator.GetObject(typeof(Subscriber.Subscriber), interestedURL);
                                remoteS.addToHistory(pub);
                            } else {
                                target = (INode)Activator.GetObject(typeof(INode), interestedURL);
                                target.addToQueue(pub);
                            }
                        }
                        break;
                    }
                }        
            }
        }

        private void shareSubRequest(string topic, string origin, Message request) {
            List<string> shareList = new List<string>();
            List<string> topicURL = null;
            INode target = null;

            foreach (String url in _parentProcessesURL) {
                if (!url.Equals(origin)) { shareList.Add(url); }
            }
            foreach (String url in _childProcessesURL) {
                if (!url.Equals(origin)) { shareList.Add(url); }
            }

            if (request.SubType.Equals(MessageType.Subscribe)) {
                _subscribersTopics.TryGetValue(topic, out topicURL);
                if (topicURL == null) {
                    topicURL = new List<string>();
                    topicURL.Add(origin);
                    _subscribersTopics.Add(topic, topicURL);
                } else {
                    _subscribersTopics[topic].Add(origin);
                }
            }
            else if (request.SubType.Equals(MessageType.Unsubscribe)) {
                _subscribersTopics.TryGetValue(topic, out topicURL);
                if (topicURL == null) {
                    Console.WriteLine("Non-Existant Topic, cannont unsubscribe...");
                    return;
                } else {
                    _subscribersTopics[topic].Remove(origin);
                }
            }

            //share request
            request.originURL = _processURL;
            foreach (string url in shareList) {
                target = (INode)Activator.GetObject(typeof(INode), url);
                target.addToQueue(request);
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
            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, uri, typeof(Broker));
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        protected override string getArguments() {
            //processName processURL site routingtype puppetMasterURL -p parentURL -c childURL -s subURL
            string arguments = _processName + " " + _processURL + " " + _site + " " + _routingPolicy + " " + _puppetMasterURL + " ";

            foreach (string parent in _parentProcessesURL) {
                arguments += " -p " + parent;
            }
            foreach (string child in _childProcessesURL) {
                arguments += " -c " + child;
            }
            foreach (string sub in _subscribers) {
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
