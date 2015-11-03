using SESDADLib;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Publisher{
    public class Publisher : Node{   
        private List<string> _siteBrokerUrl;
        private List<string> _topics;
        private List<Publication> _pubHistory;

        public Publisher(string processName, string processURL, string site, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
            _siteBrokerUrl = new List<string>();
            _topics = new List<string>();
            _pubHistory = new List<Publication>();
            
            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Publisher\\bin\\Debug\\Publisher.exe";
        }

        public void addBrokerURL(string url) {
            _siteBrokerUrl.Add(url);
        }

        public void addTopic(string topic) {
            _topics.Add(topic);
        }

        public void Publish(Publication pub) {
            _pubHistory.Add(pub);

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);
            foreach(string parentNode in _siteBrokerUrl) {
                ISubscriber sub = (ISubscriber)Activator.GetObject(typeof(ISubscriber), parentNode);
                if(sub == null)
                    System.Console.WriteLine("Could not locate parent node");
                else
                    sub.newPublication(pub);
            }
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        public override string showNode() {
            string print = "Publisher: " + _processName + " for " + _site + " active on " + _processURL + "\n";
            print += "\tConnected on broker:\n";
            foreach (string broker in _siteBrokerUrl) {
                print += "\t\t" + broker + "\n";
            }
            print += "\tPublication Topics:\n";
            foreach (string topic in _topics) {
                print += "\t\t" + topic + "\n";
            }
            print += "\tPublication History\n";
            foreach (Publication pub in _pubHistory) {
                print += "\t\t" + pub.ToString() + "\n";
            }
            return print;
        }

        protected override string getArguments() {
            //processNAme processURL site puppetMAsterURL -b brokerURL
            string text = _processName + " " + _processURL + " " + _site + " " + _puppetMasterURL;
            foreach (string broker in _siteBrokerUrl) {
                text += " -b " + broker;
            }
            return text;
        }

        public override void executeProcess() {
            _nodeProcess.StartInfo.Arguments = this.getArguments();
            _nodeProcess.Start();
            _executing = true;
        }

        public override void publishToPuppetMaster() {
            int port = Int32.Parse(_processURL.Split(':')[2].Split('/')[0]);
            string uri = _processURL.Split(':')[2].Split('/')[1];

            Console.WriteLine("Publishing on port: " + port.ToString() + " with uri: " + uri);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, uri, typeof(Publisher));
        }
    }
}
