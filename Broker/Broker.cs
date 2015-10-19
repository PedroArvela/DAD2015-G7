using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker {
    public class Broker {
        static void Main(string[] args) {
            //TODO: something
        }

        private bool routingPolicy;
        private String routingType;
        private ArrayList sites;
        private ArrayList pairedBrokers;
        private String name;
        private String processURL;
        private String parrentBroker;
        private String ChildBroker;
        private bool enabled = true;
        private bool delayed = false;
        private int delayTime = 0;

        public Broker(String url, String pURL, String cURL, int pairedBrokers, string routingType, bool enableRouting) {
            //TODO: something
        }

        public bool toggleNode() {
            return (enabled = !enabled);
        }

        public bool toggleDelay(int time) {
            delayTime = time;
            return (delayed = !delayed);
        }
    }
}
