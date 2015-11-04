﻿using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using SESDADLib;
using Subscriber;


namespace Broker {
    public class Broker : Node, IBroker {
        private bool _routingPolicy;
        private Queue<Publication> _queue;
        private Object _queueLock = new Object();

        private Dictionary<string, List<string>> _subscribersTopics = new Dictionary<string, List<string>>(); //key == topic, value == #interestedURL's
        private List<string> _subscribers = new List<string>();
        private List<string> _parentProcessesURL = new List<string>();
        private List<string> _childProcessesURL = new List<string>();

        private bool _delayed = false;
        private int _delayTime = 0;

        public Broker(string processName, string processURL, string site, string routingtype, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
            _puppetMasterURL = puppetMasterURL;
            _queue = new Queue<Publication>();
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

        private void addTopic(string topic, string interestedURL) {
            List<string> interest = null;
            bool found = false;

            Console.WriteLine("Adding to interest List...");
            if (_subscribersTopics[topic] == null) {
                interest = new List<string>();
                interest.Add(interestedURL);
                _subscribersTopics.Add(topic, interest);
                if (!_childProcessesURL.Contains(interestedURL) && !_parentProcessesURL.Contains(interestedURL)) {
                    _subscribers.Add(interestedURL);
                }
                Console.WriteLine("New topic " + interestedURL + " added with interested url " + interestedURL);
            } else {
                interest = _subscribersTopics[topic];
                foreach (string url in interest) {
                    if (interestedURL.Equals(url)) {
                        found = true;
                        Console.WriteLine("URL already on interest list");
                        break;
                    }
                }
                if (!found) {
                    Console.WriteLine("URL: " + interestedURL + " added to topic " + topic);
                    interest.Add(interestedURL);
                    if (!_childProcessesURL.Contains(interestedURL) && !_parentProcessesURL.Contains(interestedURL)) {
                        _subscribers.Add(interestedURL);
                    }
                }
            }
        }

        public void addToQueue(Publication p) {
            lock (_queueLock) {
                _queue.Enqueue(p);
            }
        }

        public void sendPublication(Publication pub) {
            Broker remoteB = null;
            Subscriber.Subscriber remoteS = null;

            
            if (!_routingPolicy) {
                foreach (string url in _childProcessesURL) {
                    //flood all available brokers
                    remoteB = (Broker)Activator.GetObject(typeof(Broker), url);
                    remoteB.addToQueue(pub);
                    Console.WriteLine("Publication sent to: " + url);
                }
            } else {
                foreach (string interestedTopic in _subscribersTopics.Keys) {
                    if (interestedTopic.Equals(pub.getTopic())) {
                        foreach (string interestedURL in _subscribersTopics[interestedTopic]) {
                            if (_subscribers.Contains(interestedURL)) {
                                remoteS = (Subscriber.Subscriber)Activator.GetObject(typeof(Subscriber.Subscriber), interestedURL);
                                remoteS.addToHistory(pub);
                            } else {
                                remoteB = (Broker)Activator.GetObject(typeof(Broker), interestedURL);
                                remoteB.addToQueue(pub);
                            }
                        }
                    }
                }        
            }
        }

        public void processQueue() {
            Publication pub = null;
            if (_queue.Count > 0 && _enabled) {
                lock (_queueLock) {
                    pub = _queue.Dequeue();
                    Console.WriteLine("Processing: " + pub.ToString());
                    //TODO: if pub is actualy a subscription request, or unsubscription call diferent function than "sendPublication"
                    this.sendPublication(pub);
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
            //processName processURL site routingtype puppetMasterURL -p parentURL -c childURL
            string arguments = _processName + " " + _processURL + " " + _site + " " + _routingPolicy + " " + _puppetMasterURL + " ";

            foreach (string parent in _parentProcessesURL) {
                arguments += " -p " + parent;
            }
            foreach (string child in _childProcessesURL) {
                arguments += " -c " + child;
            }
            return arguments;
        }

        public override void executeProcess() {
            _nodeProcess.StartInfo.Arguments = this.getArguments();
            _nodeProcess.Start();
            _executing = true;
        }

        public void subscribe(string topic) {
            throw new NotImplementedException();
        }

        public void unsubscribe(string topic) {
            throw new NotImplementedException();
        }

        public void newPublication(Publication pub) {
            throw new NotImplementedException();
        }
    }
}
