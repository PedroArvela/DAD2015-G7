using SESDADLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Publisher {
    class PublisherProgram {
        static void Main(string[] args) {
            //processNAme processURL site puppetMAsterURL -b brokerURL
            string processName = args[0];
            string processURL = args[1];
            string site = args[2];
            string puppetMasterURL = args[3];

            List<string> brokers = new List<string>();
            for (int i = 4; i < args.Length; i += 2) {
                if (args[i] == "-b") brokers.Add(args[i + 1]);
            }

            Publisher p = new Publisher(processName, processURL, site, puppetMasterURL);
            for (int i = 0; i < brokers.Count; i++) {
                p.addBrokerURL(brokers[i]);
            }
            p.publishToPuppetMaster();
            
            while (true) {
                //do nothing
            }
        }
    }
}
