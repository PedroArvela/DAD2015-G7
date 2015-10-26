using SESDADLib;
using System;
using System.Collections.Generic;

namespace Subscriber {
    public class Subscriber : Node, ISubscriber {
        private List<Publication> subscriptions;

        public Subscriber(string processName, string processURL, string site, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
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

        public override void printNode() {
            Console.WriteLine("THIS IS SUBSCRIBER");
        }
    }
}
