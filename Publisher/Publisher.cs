using SESDADLib;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Publisher{
    public class Publisher : Node{
        static void Main(string[] args) {
            //processNAme processURL site puppetMAsterURL -b brokerURL
            string processName = args[0];
            string processURL = args[1];
            string site = args[2];
            string puppetMasterURL = args[3];

            List<string> brokers = new List<string>();
            for (int i = 4; i < args.Length; i += 2) {
                if(args[i] == "-b") brokers.Add(args[i + 1]);
            }

            Publisher p = new Publisher(processName, processURL, brokers[0], puppetMasterURL);
            for (int i = 1; i < brokers.Count; i++) {
                p.addBrokerURL(brokers[i]);
            }

            //TODO: DO STUFF WITH P

            //TEST CODE
            Publisher pub = new Publisher("process1", "23232", "tcp://127.0.0.1:1337/subscriber", "232");
            pub.Publish(new Publication("191.2.3.4", "DAD", "Projecto", "Projecto de DAD", new DateTime()));
            System.Console.Read();
        }
        
        private List<string> _siteBrokerUrl;
        private List<string> _topics;
        private List<Publication> _pubHistory;
        private string _fatherNodeURL;

        public Publisher(string processName, string processURL, string fatherNodeURL, string puppetMasterURL) : base(processName, processURL, fatherNodeURL, puppetMasterURL) {
            _siteBrokerUrl = new List<string>(); // Quando tiver mais que um broker
            _topics = new List<string>();
            _pubHistory = new List<Publication>();
            
            _puppetMasterURL = puppetMasterURL;
            _fatherNodeURL = fatherNodeURL; // Apenas 1 broker

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
            ISubscriber sub = (ISubscriber)Activator.GetObject(typeof(ISubscriber), _fatherNodeURL);
            if(sub == null)
                System.Console.WriteLine("Could not locate father node");
            else
                sub.newPublication(pub);
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        public override string showNode() {
            string print = "Publisher: " + _processName + "for " + _site + " active on " + _processURL + "\n";
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
        }

        public override void OnRunCommand(String command) {
            throw new NotImplementedException();
        }
    }
}
