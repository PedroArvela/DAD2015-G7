using SESDADLib;
using System;
using System.Collections.Generic;

namespace Subscriber {
    class Subscriber : MarshalByRefObject, ISubscriber {
        private string name;
        private string processURL;
        private List<Publication> subscriptions;

        static void Main(string[] args) {
            //TODO: Something
        }

        public Subscriber() {
            //TODO: something
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
    }
}
