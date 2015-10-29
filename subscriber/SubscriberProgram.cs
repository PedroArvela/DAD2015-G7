using SESDADLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Subscriber {
    class SubscriberProgram {
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

            Subscriber p = new Subscriber(processName, processURL, brokers[0], puppetMasterURL);
            for (int i = 1; i < brokers.Count; i++) {
                p.addBrokerURL(brokers[i]);
            }

            //TODO: DO STUFF WITH P

            //TEST CODE
            Subscriber sub = new Subscriber("aaa", "tcp://localhost:1337/subscriber", "aaa", "ccc");

            TcpChannel channel = new TcpChannel(1337);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(sub, "subscriber", typeof(ISubscriber));
            Console.ReadLine();
        }
    }
}
