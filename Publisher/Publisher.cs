using SESDADLib;
using System;
using System.Collections.Generic;

namespace Publisher{
    public class Publisher : Node{
        static void Main(string[] args) {
            //TODO: something
        }
        
        private List<string> _siteBrokerUrl;

        public Publisher(string processName, string processURL, string site, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
            _puppetMasterURL = puppetMasterURL;
            _siteBrokerUrl = new List<string>();
        }

        public void addBrokerURL(string url) {
            _siteBrokerUrl.Add(url);
        }

        public void Publish(Publication pub) {
            //TODO: something
        }

        public override void printNode() {
            Console.WriteLine("THIS IS PUBLISHER");
        }
    }

    internal interface IPublisher {
    }
}
