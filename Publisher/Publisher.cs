using SESDADLib;
using System;
using System.Collections.Generic;

namespace Publisher{
    public class Publisher : Node{
        static void Main(string[] args) {
            //TODO: something
        }
        
        private List<string> _siteBrokerUrl;
        private List<string> _topics;
        private List<Publication> _pubHistory;

        public Publisher(string processName, string processURL, string site, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
            _siteBrokerUrl = new List<string>();
            _topics = new List<string>();
            _pubHistory = new List<Publication>();
            
            _puppetMasterURL = puppetMasterURL;
        }

        public void addBrokerURL(string url) {
            _siteBrokerUrl.Add(url);
        }

        public void addTopic(string topic) {
            _topics.Add(topic);
        }

        public void Publish(Publication pub) {
            _pubHistory.Add(pub);
            
            //TODO: something
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        public override string showNode() {
            string print = "Publisher: " + _processName + "for " + _site + " active on " + _processURL + "\n";
            print += "\tConnected on broker:\n";
            foreach (string broker in _siteBrokerUrl) {
                print += "\t\t" + broker + "\n";
            }
            print += "\tPublication Topics:\n";
            foreach (string topic in _topics) {
                print += "\t\t" + topic + "\n";
            }
            print += "\tPublication History\n";
            foreach (Publication pub in _pubHistory) {
                print += "\t\t" + pub.ToString() + "\n";
            }
            return print;
        }
    }

    internal interface IPublisher {
    }
}
