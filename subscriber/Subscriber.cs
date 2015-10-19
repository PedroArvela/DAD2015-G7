using System;
using System.Collections.Generic;

namespace Subscriber {
    class Subscriber : MarshalByRefObject {
        private string name;
        private string processURL;
        private List<string> subscriptions;
        //TODO: create type for subscriptions

        public Subscriber() {
            //TODO: something
        }

        public void subscribe(String site) {
            //TODO: something
        }

        public void unSubscribe(String site) {
            //TODO: something
        }

        // Callback for brokers to use to send the data
        public void newPublication(string topic, string content) {
            //TODO: something
        }
    }
}
