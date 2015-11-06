﻿using SESDADLib;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Subscriber {
    public class Subscriber : Node, INode {
        private int sendSequence = 0;
        private string _ordering;
        private List<string> _subscriptionTopics;
        private Dictionary<string, List<Message>> _subscriptionHistory; //key -> topic | value -> history
        private List<string> _siteBrokerUrl;
        private object _queueLock = new object();

        public Subscriber(string processName, string processURL, string site, string puppetMasterURL, string ordering) : base(processName, processURL, site, puppetMasterURL) {
            _subscriptionTopics = new List<string>();
            _subscriptionHistory = new Dictionary<string, List<Message>>();
            _siteBrokerUrl = new List<string>();
            _ordering = ordering;

            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Subscriber\\bin\\Debug\\Subscriber.exe";
        }

        public void setOrdering(String order) { _ordering = order; }

        public void addTopic(string topic) {
            _subscriptionTopics.Add(topic);
        }

        public void selfRegister() {
            Console.WriteLine("Subscriber registered on port: " + _port.ToString() + " with uri: " + _uriAddress);

            TcpChannel channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, _uriAddress, typeof(INode));
        }

        public void addToQueue(Message msg) {
            Console.WriteLine(msg.Content);
            lock (_queueLock) {
                addToHistory(msg);
            }
        }

        public void addToHistory(Message pub) {
            string topic = pub.Topic;
            List<Message> messages = null;

            _subscriptionHistory.TryGetValue(topic, out messages);
            if (messages == null) {
                _subscriptionHistory.Add(topic, new List<Message>());
                _subscriptionHistory[topic].Add(pub);
            } else {
                _subscriptionHistory[topic].Add(pub);
            }
        }

        public void subscribe(string topic) {
            INode broker = null;
            Message request = null;

            foreach (string brokerURL in _siteBrokerUrl)
            {
                broker = (INode)Activator.GetObject(typeof(INode), brokerURL);
                if (broker == null) {
                    Console.WriteLine("Could not connect to broker: " + brokerURL);
                } else if(_subscriptionTopics.Contains(topic)) {
                    Console.WriteLine("Topic already Subscribed to...");
                } else {
                    Console.WriteLine("Sending subscription request to: " + brokerURL);
                    request = new Message(MessageType.Subscribe, _site, topic, "subscribe", DateTime.Now, sendSequence);
                    request.originURL = _processURL;
                    broker.addToQueue(request);
                    _subscriptionTopics.Add(topic);
                    sendSequence++;
                }
            }
        }

        public void unsubscribe(string topic) {
            INode broker = null;
            Message request = null;

            foreach (string brokerURL in _siteBrokerUrl)
            {
                broker = (INode)Activator.GetObject(typeof(INode), brokerURL);
                if (broker == null) {
                    Console.WriteLine("Could not connect to broker: " + brokerURL);
                } else if (!_subscriptionTopics.Contains(topic)) {
                    Console.WriteLine("Non-existant Topic...");
                } else {
                    Console.WriteLine("Sending unsubscription request to: " + brokerURL);
                    request = new Message(MessageType.Unsubscribe, _site, topic, "unsubscribe", DateTime.Now, sendSequence);
                    request.originURL = _processURL;
                    broker.addToQueue(request);
                    _subscriptionTopics.Remove(topic);
                    sendSequence++;
                }
            }
        }

        public void addBrokerURL(string url) {
            _siteBrokerUrl.Add(url);
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        public override string showNode() {
            String print = "Subscriber: " + _processName + " for " + _site + " active on " + _processURL + "\n";
            print += "Ordering: " + _ordering + "\n";
            print += "\tConnected on broker:\n";
            foreach (string broker in _siteBrokerUrl) {
                print += "\t\t";
                print += broker;
                print += "\n";
            }
            print += "\tSubscribed to:\n";
            foreach (string topic in _subscriptionTopics) {
                print += "\t\t" + topic + "\n";
            }
            print += "\tSubscription History\n";
            foreach (KeyValuePair<string, List<Message>> entry in _subscriptionHistory) {
                print += "\t\t" + entry.Key + "\n";
                foreach (Message pub in entry.Value) {
                    print += "\t\t\t" + pub.ToString();
                }
            }
            return print;
        }

        protected override string getArguments() {
            //processNAme processURL site puppetMAsterURL ordering -b brokerURL
            string text = _processName + " " + _processURL + " " + _site + " " + _puppetMasterURL + " " + _ordering;
            foreach(string broker in _siteBrokerUrl) {
                text += " -b " + broker;
            }
            return text;
        }

        public override void executeProcess() {
            _nodeProcess.StartInfo.Arguments = this.getArguments();
            _nodeProcess.Start();
            _executing = true;
        }

        public override void publishToPuppetMaster() {
            int port = Int32.Parse(_processURL.Split(':')[2].Split('/')[0]);
            string uri = _processURL.Split(':')[2].Split('/')[1];

            Console.WriteLine("Publishing on port: " + port.ToString() + " with uri: " + uri);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, uri, typeof(Subscriber));
        }
    }
}
