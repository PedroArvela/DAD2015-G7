using SESDADLib;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace Publisher {
    public class Publisher : Node {
        private string siteBrokerUrl = "";
        private INode siteBroker;
        private List<string> _topics;
        private List<Message> _pubHistory;
        private Object _sendLock = new Object();
        private int _sendSequence;

        public Publisher(string processName, string processURL, string site, string puppetMasterURL) : base(processName, processURL, site, puppetMasterURL) {
            _topics = new List<string>();
            _pubHistory = new List<Message>();
            _sendSequence = 0;

            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Publisher\\bin\\Debug\\Publisher.exe";
        }

        public void addBrokerURL(string url) {
            INode node = aquireConnection(url);
            siteBrokerUrl = url;
            siteBroker = node;
        }

        public void addTopic(string topic) {
            _topics.Add(topic);
        }

        public void addPublishRequest(string topic, int times, int intervalMS) {
                Console.WriteLine("New publication request");
                Thread publishTask = new Thread(() => Publish(topic, times, intervalMS));
                publishTask.Start();
        }

        private void Publish(string topic, int numberOfEvents, int intervalMS) {
            Message pub = null;
            int sequence = -1;

            for (int i = 0; i < numberOfEvents; i++) {
                lock (_sendLock) {
                    sequence = _sendSequence;
                    _sendSequence++;

                    Console.WriteLine("Publish event #" + i + " out of " + numberOfEvents + ". Using sequence #" + sequence);

                    pub = new Message(MessageType.Publication, _site, topic, "publication", DateTime.Now, sequence, _processName);
                    pub.originURL = _processURL;
                    _pubHistory.Add(pub);

                    if (siteBroker == null)
                        Console.WriteLine("Failed to connect to broker");
                    else {
                        Console.WriteLine("Trying to send sequence #" + sequence);
                        siteBroker.addToQueue(pub);
                        Console.WriteLine("PubEvent " + _processName + ", " + _processURL + ", " + topic + ", " + sequence);
                        this.writeToLog("PubEvent " + _processName + ", " + _processURL + ", " + topic + ", " + sequence);
                    }

                    Console.WriteLine("Sleeping for " + intervalMS + "ms");
                    Thread.Sleep(intervalMS);
                }
            }
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        public override string showNode() {
            string print = "Publisher: " + _processName + " for " + _site + " active on " + _processURL + "\n";
            print += "\tConnected on broker:\n";
            if (siteBroker != null) {
                print += "\t\t" + siteBroker.Url() + "\n";
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
            if (siteBrokerUrl != "") {
                text += " -b " + siteBrokerUrl;
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

            RemotingServices.Marshal(this, uri, typeof(Publisher));
        }
    }
}
