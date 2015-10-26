using SESDADLib;
using System;
using System.Collections.Generic;

namespace Subscriber {
    public class Subscriber : Node, ISubscriber {
        private List<string> _subscriptionTopics;
        private Dictionary<string, List<Publication>> _subscriptionHistory; //key -> topic | value -> history
        private List<string> _siteBrokerUrl;

        public Subscriber(string processName, string processURL, string site, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
            _subscriptionTopics = new List<string>();
            _subscriptionHistory = new Dictionary<string, List<Publication>>();
            _siteBrokerUrl = new List<string>();
        }

        public void addTopic(string topic) { _subscriptionTopics.Add(topic); }
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
            //TODO: something
        }

        public void unsubscribe(string topic) {
            //TODO: something
        }

        // Callback for brokers to use to send the data
        public void newPublication(Publication pub) {
            //TODO: something
        }

        public void addBrokerURL(string url) {
            _siteBrokerUrl.Add(url);
        }

        public override void printNode() {
            String print = "Subscriber: " + _processName + "for " + _site + " active on " + _processURL + "\n";
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
            foreach(KeyValuePair<string, List<Publication>> entry in _subscriptionHistory) {
                print += "\t\t" + entry.Key + "\n";
                foreach (Publication pub in entry.Value) {
                    print += "\t\t\t" + pub.ToString();
                }
            }
            Console.WriteLine(print);
        }
    }
}
