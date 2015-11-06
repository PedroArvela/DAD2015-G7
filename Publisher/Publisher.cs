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
        private List<Message> _pubHistory;
        private int _sendSequence;

        public Publisher(string processName, string processURL, string site, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
            _siteBrokerUrl = new List<string>();
            _topics = new List<string>();
            _pubHistory = new List<Message>();
            _sendSequence = 0;

            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Publisher\\bin\\Debug\\Publisher.exe";
        }

        public void addBrokerURL(string url) {
            _siteBrokerUrl.Add(url);
        }

        public void addTopic(string topic) {
            _topics.Add(topic);
        }

        public void Publish(string topic) {
            INode target = null;
            Message pub = new Message(MessageType.Publication, _site, topic, "publication", DateTime.Now, _sendSequence);
            this._pubHistory.Add(pub);
            pub.originURL = _processURL;
            _sendSequence++;

            foreach(string url in _siteBrokerUrl) {
                target = (INode)Activator.GetObject(typeof(INode), url);
                if (target == null)
                    System.Console.WriteLine("Failed to connect to " + url);
                else {
                    target.addToQueue(pub);
                }
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
            foreach (Message pub in _pubHistory) {
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
