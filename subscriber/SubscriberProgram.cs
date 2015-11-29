using System.Collections.Generic;
using System.Threading;

namespace Subscriber {
    class SubscriberProgram {
        static void Main(string[] args) {
            //processNAme processURL site puppetMAsterURL -b brokerURL
            string processName = args[0];
            string processURL = args[1];
            string site = args[2];
            string puppetMasterURL = args[3];
            string ordering = args[4];

            List<string> brokers = new List<string>();
            for (int i = 5; i < args.Length; i += 2) {
                if (args[i] == "-b") brokers.Add(args[i + 1]);
            }

            Subscriber s = new Subscriber(processName, processURL, site, puppetMasterURL, ordering);
            for (int i = 0; i < brokers.Count; i++) {
                s.addBrokerURL(brokers[i]);
            }
            s.publishToPuppetMaster();

            while (true) {
                s.processQueue();
                Thread.Sleep(1000);
            }
        }
    }
}
