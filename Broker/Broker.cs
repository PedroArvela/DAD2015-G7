using SESDADLib;
using System;
using System.Collections.Generic;

namespace Broker {
    public class Broker : MarshalByRefObject, IBroker {
        private bool routingPolicy;
        private string routingType;
        private List<string> sites;
        private List<string> pairedBrokers;
        private string name;
        private string processURL;
        private string parrentBroker;
        private string ChildBroker;
        private bool enabled = true;
        private bool delayed = false;
        private int delayTime = 0;

        public Broker(string url, string pURL, string cURL, int pairedBrokers, string routingType, bool enableRouting) {
            //TODO: something
        }

        public bool toggleNode() {
            return (enabled = !enabled);
        }

        public bool toggleDelay(int time) {
            delayTime = time;
            return (delayed = !delayed);
        }

        public void newPublication(Publication pub) {
            //TODO: something
        }

        public void sendPublication(Publication pub) {
            //TODO: something
        }
    }
}
