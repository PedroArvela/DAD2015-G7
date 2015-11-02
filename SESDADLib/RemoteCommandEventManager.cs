using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDADLib {
    public class RemoteCommandEventManager : MarshalByRefObject {
        private string _url;
        private int _port;
        private string _ip;
        private string _name;

        //Event handler for any remote command
        public delegate void runCommandEventHandler(string command);
        //event to run whenever a remote command is published
        public event runCommandEventHandler ranCommand;

        public RemoteCommandEventManager(string url) {
            _url = url;
            string[] u = url.Split(':');
            _ip = u[0].Substring(6);

            u = u[1].Split('/');
            _port = Int32.Parse(u[0]);
            _name = u[1];
        }
        public RemoteCommandEventManager(string ip, int port, string name) {
            _ip = ip;
            _port = port;
            _name = name;
            _url = "tcp://" + _ip + ":" + _port.ToString() + "/" + _name;
        }

        public void subscribe(Node n) {
            //FIXME: this could lead to problems where remote object can't reach event subscriber
            this.ranCommand += n.OnRunCommand;
        }

        public void announceEventManager() {
            TcpChannel channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(channel,false);

            RemotingServices.Marshal(this, _name, typeof(RemoteCommandEventManager));
        }

        public void OnRunCommand(string command) {
            if (this.ranCommand != null) {
                this.ranCommand(command);
            }
        }
    }
}
