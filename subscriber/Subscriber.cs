using SESDADLib;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Subscriber {
    public class Subscriber : Node, INode {
        private int eventNumber = 0;
        private int sendSequence = 0;
        private string _ordering;
        private List<string> _subscriptionTopics;
        private Dictionary<string, int> _lastDelivered; // key: pubURI
        private Queue<Message> _queueMessages;
        private Dictionary<string, Dictionary<int, Message>> _undeliveredList; // key: pubURI, value: undeliveredList
        private List<Message> _messageHistory;
        private List<string> _siteBrokerUrl;
        private object _queueLock = new object();

        public Subscriber(string processName, string processURL, string site, string puppetMasterURL, string ordering) : base(processName, processURL, site, puppetMasterURL) {
            _subscriptionTopics = new List<string>();
            _lastDelivered = new Dictionary<string, int>();
            _queueMessages = new Queue<Message>();
            _undeliveredList = new Dictionary<string, Dictionary<int, Message>>();
            _messageHistory = new List<Message>();
            _siteBrokerUrl = new List<string>();
            _ordering = ordering;

            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Subscriber\\bin\\Debug\\Subscriber.exe";
        }

        public void setOrdering(String order) { _ordering = order; }

        public void addTopic(string topic) {
            _subscriptionTopics.Add(topic);
        }

        public void selfRegister() {
            Console.WriteLine("Subscriber registered on port: " + _port.ToString() + " with uri: " + _uriAddress);

            TcpChannel channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, _uriAddress, typeof(INode));
        }

        public void addToQueue(Message msg) {
            Console.WriteLine(msg.Content);
            lock (_queueLock) {
                addToHistory(msg);
            }
        }

        public void addToHistory(Message pub) {
            string topic = pub.Topic;

            Console.WriteLine("New unordered message on topic " + pub.Topic + " from " + pub.originURL + " sequence " + pub.Sequence);
            _queueMessages.Enqueue(pub);
        }

        public void processQueue() {
            Message pub;
            lock (_queueLock) {
                if (_queueMessages.Count == 0)
                    return;

                pub = _queueMessages.Dequeue();
            }

            // TODO: Move this to addBrokerURL
            string origin = pub.originURL;
            int seq = pub.Sequence;

            Dictionary<int, Message> messages = null;
            _undeliveredList.TryGetValue(origin, out messages);
            if (messages == null) {
                _undeliveredList.Add(origin, new Dictionary<int, Message>());
            }

            if (!_lastDelivered.ContainsKey(origin)) {
                _lastDelivered.Add(origin, -1);
            }

            // No ordering
            if (_ordering == "NO") {
                Console.WriteLine("Delivering message on topic " + pub.Topic + " from " + pub.originURL + " sequence " + pub.Sequence);
                writeToLog("SubEvent " + _processName + ", " + pub.Publisher + ", " + pub.Topic + ", " + eventNumber);
                eventNumber++;

                _messageHistory.Add(pub);
                _lastDelivered[origin] = pub.Sequence;
                return;
            }

            // FIFO ordering
            Console.WriteLine("Processing message on topic " + pub.Topic + " from " + pub.originURL + " sequence " + pub.Sequence);

            Console.WriteLine("Sequence: " + seq + "\tLast: " + _lastDelivered[origin]);
            if (_lastDelivered[origin] == seq - 1) {
                Console.WriteLine("Delivering message on topic" + pub.Topic + " from " + pub.originURL + " sequence " + pub.Sequence);
                writeToLog("SubEvent " + _processName + ", " + pub.Publisher + ", " + pub.Topic + ", " + eventNumber);
                eventNumber++;

                _messageHistory.Add(pub);
                _lastDelivered[origin] = seq;

                while (_undeliveredList[origin].ContainsKey(_lastDelivered[origin] + 1)) {
                    pub = _undeliveredList[origin][_lastDelivered[origin] + 1];
                    _undeliveredList[origin].Remove(_lastDelivered[origin] + 1);

                    _messageHistory.Add(pub);

                    _lastDelivered[origin] += 1;
                }
            } else if (_lastDelivered[origin] > seq) {
                _undeliveredList[origin].Add(pub.Sequence, pub);
            }
        }

        public void subscribe(string topic) {
            INode broker = null;
            Message request = null;

            foreach (string brokerURL in _siteBrokerUrl)
            {
                broker = (INode)Activator.GetObject(typeof(INode), brokerURL);
                if (broker == null) {
                    Console.WriteLine("Could not connect to broker: " + brokerURL);
                } else if(_subscriptionTopics.Contains(topic)) {
                    Console.WriteLine("Topic already Subscribed to...");
                } else {
                    Console.WriteLine("Sending subscription request to: " + brokerURL);
                    writeToLog("Subscriber " + _processName + " Subscribe " + topic);

                    request = new Message(MessageType.Subscribe, _site, topic, "subscribe", DateTime.Now, sendSequence, _processName);
                    request.originURL = _processURL;
                    broker.addToQueue(request);
                    _subscriptionTopics.Add(topic);
                    sendSequence++;
                }
            }
        }

        public void unsubscribe(string topic) {
            INode broker = null;
            Message request = null;

            foreach (string brokerURL in _siteBrokerUrl)
            {
                broker = (INode)Activator.GetObject(typeof(INode), brokerURL);
                if (broker == null) {
                    Console.WriteLine("Could not connect to broker: " + brokerURL);
                } else if (!_subscriptionTopics.Contains(topic)) {
                    Console.WriteLine("Non-existant Topic...");
                } else {
                    Console.WriteLine("Sending unsubscription request to: " + brokerURL);
                    writeToLog("Subscriber " + _processName + " Unsubscribe " + topic);

                    request = new Message(MessageType.Unsubscribe, _site, topic, "unsubscribe", DateTime.Now, sendSequence, _processName);
                    request.originURL = _processURL;
                    broker.addToQueue(request);
                    _subscriptionTopics.Remove(topic);
                    sendSequence++;
                }
            }
        }

        public void addBrokerURL(string url) {
            _siteBrokerUrl.Add(url);
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        public override string showNode() {
            String print = "Subscriber: " + _processName + " for " + _site + " active on " + _processURL + "\n";
            print += "Ordering: " + _ordering + "\n";
            print += "\tConnected on broker:\n";
            foreach (string broker in _siteBrokerUrl) {
                print += "\t\t";
                print += broker;
                print += "\n";
            }
            print += "\tSubscribed to:\n";
            foreach (string topic in _subscriptionTopics) {
                print += "\t\t" + topic + "\n";
            }
            print += "\tSubscription History\n";
            foreach (Message pub in _messageHistory) {
                print += "\t\t\t" + pub.ToString() + "\n";
            }
            return print;
        }

        protected override string getArguments() {
            //processNAme processURL site puppetMAsterURL ordering -b brokerURL
            string text = _processName + " " + _processURL + " " + _site + " " + _puppetMasterURL + " " + _ordering;
            foreach(string broker in _siteBrokerUrl) {
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

            Console.WriteLine("\t\tPublishing on port: " + port.ToString() + " with uri: " + uri + "\n");

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, uri, typeof(Subscriber));
        }
    }
}
