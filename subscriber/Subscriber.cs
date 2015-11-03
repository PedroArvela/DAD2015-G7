﻿using SESDADLib;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Subscriber {
    public class Subscriber : Node, ISubscriber {
        private List<string> _subscriptionTopics;
        private Dictionary<string, List<Publication>> _subscriptionHistory; //key -> topic | value -> history
        private List<string> _siteBrokerUrl;

        public Subscriber(string processName, string processURL, string site, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
            _subscriptionTopics = new List<string>();
            _subscriptionHistory = new Dictionary<string, List<Publication>>();
            _siteBrokerUrl = new List<string>();

            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Subscriber\\bin\\Debug\\Subscriber.exe";
            //FIXME: selfRegister();
        }

        public void addTopic(string topic) {
            _subscriptionTopics.Add(topic);
        }

        public void selfRegister()
        {
            TcpChannel channel = new TcpChannel(1337);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, "subscriber", typeof(ISubscriber));
        }

        public void addToHistory(Publication pub) {
            string topic = pub._topic;
            if (_subscriptionHistory[topic] == null) {
                _subscriptionHistory.Add(topic, new List<Publication>());
                _subscriptionHistory[topic].Add(pub);
            } else {
                _subscriptionHistory[topic].Add(pub);
            }
        }

        public void subscribe(string topic) {
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);
            foreach (string parentNode in _siteBrokerUrl)
            {
                IBroker broker = (IBroker)Activator.GetObject(typeof(ISubscriber), parentNode);
                if (broker == null)
                    System.Console.WriteLine("Could not locate parent node");
                else
                    broker.subscribe(topic);
            }
        }

        public void unsubscribe(string topic) {
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);
            foreach (string parentNode in _siteBrokerUrl)
            {
                IBroker broker = (IBroker)Activator.GetObject(typeof(ISubscriber), parentNode);
                if (broker == null)
                    System.Console.WriteLine("Could not locate parent node");
                else
                    broker.unsubscribe(topic);
            }
        }

        // Callback for brokers to use to send the data
        public void newPublication(Publication pub) {
            addToHistory(pub);
            Console.WriteLine(pub._content);
        }

        public void addBrokerURL(string url) {
            _siteBrokerUrl.Add(url);
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        public override string showNode() {
            String print = "Subscriber: " + _processName + " for " + _site + " active on " + _processURL + "\n";
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
            foreach (KeyValuePair<string, List<Publication>> entry in _subscriptionHistory) {
                print += "\t\t" + entry.Key + "\n";
                foreach (Publication pub in entry.Value) {
                    print += "\t\t\t" + pub.ToString();
                }
            }
            return print;
        }

        protected override string getArguments() {
            //processNAme processURL site puppetMAsterURL -b brokerURL
            string text = _processName + " " + _processURL + " " + _site + " " + _puppetMasterURL;
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
