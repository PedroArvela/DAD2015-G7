using System.Collections.Generic;
using System.Threading;

namespace Broker {
    class BrokerProgram {
        static void Main(string[] args) {
            //TODO: validade input

            //processName processURL site routingtype puppetMasterURL -p parentURL -c childURL
            string processName = args[0];
            string processURL = args[1];
            string site = args[2];
            string routing = args[3];
            string puppetMasterURL = args[4];
            string loggingLevel = args[5];

            List<string> childList = new List<string>();
            List<string> parentList = new List<string>();
            List<string> subList = new List<string>();

            for (int i = 6; i < args.Length; i += 2) {
                switch (args[i]) {
                    case "-p":
                        parentList.Add(args[i + 1]);
                        break;
                    case "-c":
                        childList.Add(args[i + 1]);
                        break;
                    case "-s":
                        subList.Add(args[i + 1]);
                        break;
                }
            }

            //initialization of data finished
            Broker b = new Broker(processName, processURL, site, routing, puppetMasterURL, loggingLevel);
            foreach (string c in childList) {
                b.addChildUrl(c);
            }
            foreach (string p in parentList) {
                b.addParentUrl(p);
            }
            foreach (string s in subList) {
                b.addSubscriberUrl(s);
            }
            b.publishToPuppetMaster();

            //Broker program Logic
            while (true) {
                b.processQueue();
                Thread.Sleep(1000);
            }
        }
    }
}
