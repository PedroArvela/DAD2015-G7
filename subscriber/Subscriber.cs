using SESDADLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;

namespace Subscriber {
    public class Subscriber : Node, INode {
        private int eventNumber = 0;
        private int sendSequence = 0;
        private string _ordering;
        private List<string> _subscriptionTopics = new List<string>();
        private Dictionary<string, int> _lastDelivered = new Dictionary<string, int>(); // key: pubURI
        private ConcurrentQueue<Message> _queueMessages = new ConcurrentQueue<Message>();
        private Dictionary<string, Dictionary<int, Message>> _undeliveredList = new Dictionary<string, Dictionary<int, Message>>(); // key: pubURI, value: undeliveredList
        private List<Message> _messageHistory = new List<Message>();
        private Dictionary<string, INode> parents = new Dictionary<string, INode>();
        private string mainParentURL = "";
        private INode mainParent;
        private object _queueLock = new object();

        // Total Message Ordering
        private int lastDeliveredTotal = -1;
        private Dictionary<int, Message> undeliveredListTotal = new Dictionary<int, Message>();

        public Subscriber(string processName, string processURL, string site, string puppetMasterURL, string ordering) : base(processName, processURL, site, puppetMasterURL) {
            _ordering = ordering;

            _nodeProcess.StartInfo.FileName = "..\\..\\..\\Subscriber\\bin\\Debug\\Subscriber.exe";
        }

        public void setOrdering(String order) {
            Console.WriteLine("Setting ordering to " + order);
            _ordering = order;
        }

        public void addTopic(string topic) {
            _subscriptionTopics.Add(topic);
        }

        public void selfRegister() {
            Console.WriteLine("Subscriber registered on port: " + _port.ToString() + " with uri: " + _uriAddress);

            TcpChannel channel = new TcpChannel(_port);

            RemotingServices.Marshal(this, _uriAddress, typeof(INode));
        }

        public void addToQueue(Message msg) {
            Console.WriteLine("New unordered message on topic " + msg.Topic + " from " + msg.Publisher + " sequence " + msg.Sequence);
            _queueMessages.Enqueue(msg);
        }

        public void processQueue() {
            Message pub;
            if (!_queueMessages.TryDequeue(out pub)) {
                return;
            }

            Console.WriteLine("Processing message on topic " + pub.Topic + " from " + pub.Publisher + " sequence " + pub.Sequence);

            string origin = pub.Publisher;

            // Create the data structures to manage the messages of the publisher if they don't exist yet
            if (!_undeliveredList.ContainsKey(origin)) {
                _undeliveredList.Add(origin, new Dictionary<int, Message>());
                _lastDelivered.Add(origin, pub.Sequence - 1);
            }

            if (_ordering == "NO") {
                deliverUnordered(pub);
            } else if (_ordering == "FIFO") {
                deliverFifo(pub);
            } else if (_ordering == "TOTAL") {
                deliverTotal(pub);
            }
        }

        private void deliverUnordered(Message pub) {
            string origin = pub.Publisher;
            Console.WriteLine("Delivering message on topic " + pub.Topic + " from " + pub.Publisher + " sequence " + pub.Sequence);
            writeToLog("SubEvent " + _processName + ", " + pub.Publisher + ", " + pub.Topic + ", " + eventNumber);
            eventNumber++;

            _messageHistory.Add(pub);
            _lastDelivered[origin] = pub.Sequence;
        }

        private void deliverFifo(Message pub) {
            string origin = pub.Publisher;
            int seq = pub.Sequence;

            if (_lastDelivered[origin] == seq - 1) {
                // Deliver only if it is the next in the sequence
                Console.WriteLine("Delivering message on topic" + pub.Topic + " from " + origin + " sequence " + seq);
                writeToLog("SubEvent " + _processName + ", " + origin + ", " + pub.Topic + ", " + eventNumber);
                eventNumber++;

                _messageHistory.Add(pub);
                _lastDelivered[origin] = seq;

                // Deliver all undelivered messages depending on this one
                while (_undeliveredList[origin].ContainsKey(seq + 1)) {
                    seq = _lastDelivered[origin] + 1;
                    pub = _undeliveredList[origin][seq];

                    _undeliveredList[origin].Remove(seq);
                    _messageHistory.Add(pub);
                    _lastDelivered[origin] += 1;
                }
            } else if (_lastDelivered[origin] > seq) {
                // Otherwise just store it for future delivery
                _undeliveredList[origin].Add(pub.Sequence, pub);
            }
        }

        private void deliverTotal(Message pub) {
            int seq = pub.Order;

            if (lastDeliveredTotal == seq - 1) {
                // Deliver only if it is the next in the sequence
                Console.WriteLine("Delivering message #" + seq + " on topic" + pub.Topic + " from " + pub.Publisher + " sequence " + pub.Sequence);
                writeToLog("SubEvent " + _processName + ", " + pub.Publisher + ", " + pub.Topic + ", " + eventNumber);
                eventNumber++;

                _messageHistory.Add(pub);
                lastDeliveredTotal = seq;

                // Deliver all undelivered messages depending on this one
                while (undeliveredListTotal.ContainsKey(seq + 1)) {
                    seq = lastDeliveredTotal + 1;
                    pub = undeliveredListTotal[seq];

                    undeliveredListTotal.Remove(seq);
                    _messageHistory.Add(pub);
                    lastDeliveredTotal++;
                }
            } else if (lastDeliveredTotal > seq) {
                // Otherwise just store it for future delivery
                undeliveredListTotal.Add(seq, pub);
            }
        }

        public void subscribe(string topic) {
            Message request = null;

            if (mainParent == null) {
                Console.WriteLine("Could not connect to broker");
            } else if (_subscriptionTopics.Contains(topic)) {
                Console.WriteLine("Topic already Subscribed to...");
            } else {
                Console.WriteLine("Sending subscription request to: " + mainParentURL);
                writeToLog("Subscriber " + _processName + " Subscribe " + topic);

                request = new Message(MessageType.Subscribe, _site, _processName, topic, sendSequence);
                try {
                    request.Sender = _processURL;
                    request.SenderSite = getSite();
                    mainParent.addToQueue(request);
                    _subscriptionTopics.Add(topic);
                    sendSequence++;
                } catch(System.Net.Sockets.SocketException)
                {
                    parents.Remove(mainParentURL);
                    List<string> keyList = new List<string>(parents.Keys);
                    mainParentURL = keyList[0];
                    Console.WriteLine(mainParentURL + " was unavailable, switching to " + keyList[0]);
                    mainParent = parents[mainParentURL];
                    subscribe(topic);
                }
            }
        }

        public void unsubscribe(string topic) {
            Message request = null;

            if (mainParent == null) {
                Console.WriteLine("Could not connect to broker");
            } else if (!_subscriptionTopics.Contains(topic)) {
                Console.WriteLine("Nonexistent Topic...");
            } else {
                Console.WriteLine("Sending unsubscription request to: " + mainParentURL);
                writeToLog("Subscriber " + _processName + " Unsubscribe " + topic);

                request = new Message(MessageType.Unsubscribe, _site, _processName, topic, sendSequence);
                request.Sender = _processURL;
                request.SenderSite = getSite();
                mainParent.addToQueue(request);
                _subscriptionTopics.Remove(topic);
                sendSequence++;
            }
        }

        public void addBrokerURL(string url) {
            INode node = aquireConnection(url);
            if(mainParent == null)
            {
                mainParent = node;
                mainParentURL = url;
            }
            parents.Add(url, node);
        }

        public override void printNode() {
            Console.WriteLine(this.showNode());
        }

        public override string showNode() {
            String print = "Subscriber: " + _processName + " for " + _site + " active on " + _processURL + "\n";
            print += "Ordering: " + _ordering + "\n";
            print += "\tConnected on brokers:\n";
            foreach (string broker in parents.Keys)
            {
                print += "\t\t" + broker + "\n";
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

            foreach(string url in parents.Keys) {
                text += " -b " + url;
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

            Console.WriteLine("Publishing on port: " + port.ToString() + " with uri: " + uri + "\n");

            TcpChannel channel = new TcpChannel(port);

            RemotingServices.Marshal(this, uri, typeof(Subscriber));
        }
    }
}
