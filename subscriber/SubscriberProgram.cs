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
            Subscriber sub = new Subscriber("aaa", "tcp://localhost:1337/subscriber", "aaa", "ccc");

            TcpChannel channel = new TcpChannel(1337);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(sub, "subscriber", typeof(ISubscriber));
            Console.ReadLine();
        }
    }
}
