using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using SESDADLib;


namespace Broker {
    public class Broker : Node, IBroker {
        private bool _routingPolicy;
        
        private Dictionary<string, int> _subscribersTopics = new Dictionary<string, int>(); //key == topic, value == #subscribers
        private List<string> _parentProcessesURL = new List<string>();
        private List<string> _childProcessesURL = new List<string>();
        
        private bool _delayed = false;
        private int _delayTime = 0;

        public Broker(string processName, string processURL, string site, string routingtype, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
            _puppetMasterURL = puppetMasterURL;
            switch (routingtype) {
                case "flooding":
                    _routingPolicy = false;
                    break;
                case "filter":
                    _routingPolicy = true;
                    break;
            }

            //process start arguments
            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Broker\\bin\\Debug\\Broker.exe";
        }
        
        public List<string> getParentURL() { return _parentProcessesURL; }
        public List<string> getChildURL() { return _childProcessesURL; }

        public void addChildUrl(string url) {
            _childProcessesURL.Add(url);
        }
        public void addParentUrl(string url) {
            _parentProcessesURL.Add(url);
        }

        public bool toggleNode() {
            return (_enabled = !_enabled);
        }

        public bool toggleDelay(int time) {
            _delayTime = time;
            return (_delayed = !_delayed);
        }

        public override string showNode() {
            string print = "\tBroker: " + _processName + " for " + _site + " active on " + _processURL + "\n";
            print += "\tParent Broker URL(s):\n";
            foreach (string purl in _parentProcessesURL) {
                print += "\t\t" + purl + "\n";
            }
            print += "\tChild Broker URL(s):\n";
            foreach (string curl in _childProcessesURL) {
                print += "\t\t" + curl + "\n";
            }
            print += "\tTopic(s):\n";
            foreach (string topic in _subscribersTopics.Keys) {
                print += "\t\t" + topic + "with " + _subscribersTopics[topic].ToString() + "subscribers\n";
            }
            return print;
        }

        public void newPublication(Publication pub) {
            throw new NotImplementedException();
        }

        public void sendPublication(Publication pub) {
            throw new NotImplementedException();
        }

        public override void publishToPuppetMaster() {
            int port = Int32.Parse(_processURL.Split(':')[2].Split('/')[0]);
            string uri = _processURL.Split(':')[2].Split('/')[1];

            Console.WriteLine("Publishing on port: " + port.ToString() + " with uri: " + uri);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, uri, typeof(Broker));
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        protected override string getArguments() {
            //processName processURL site routingtype puppetMasterURL -p parentURL -c childURL
            string arguments = _processName + " " + _processURL + " " + _site + " " + _routingPolicy + " " + _puppetMasterURL + " ";

            foreach (string parent in _parentProcessesURL) {
                arguments += " -p " + parent;
            }
            foreach (string child in _childProcessesURL) {
                arguments += " -c " + child;
            }
            return arguments;
        }

        public override void executeProcess() {
            _nodeProcess.StartInfo.Arguments = this.getArguments();
            _nodeProcess.Start();
            _executing = true;
        }

        public void subscribe(string topic) {
            throw new NotImplementedException();
        }

        public void unsubscribe(string topic) {
            throw new NotImplementedException();
        }
    }
}
