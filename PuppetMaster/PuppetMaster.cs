using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Element;
using SESDADLib;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetMaster : MarshalByRefObject, IPuppetMaster {

        private List<Broker.Broker> _brokers;
        private List<Publisher.Publisher> _publishers;
        private List<Subscriber.Subscriber> _subscribers;
        private element networkTree = null; //holds tree for elements in network

        private Broker.Broker _remoteBroker = null;
        private Publisher.Publisher _remotePub = null;
        private Subscriber.Subscriber _remoteSub = null;

        private Object _logLock = new Object();

        private string _loggingLevel = "light";
        private bool _routingPolicy = false;
        private String logFile = ".\\Logfile.txt";
        private String _ordering = "NO";
        private String _masterURL = "";
        private StreamWriter logFilePipe;

        static void Main(string[] args) {
            PuppetMaster master = new PuppetMaster("tcp://localhost:1337/puppetMaster");
            bool open = true;

            Console.WriteLine("Welcome!\n" + Directory.GetCurrentDirectory());
            while (open) {
                Console.Write("#: ");
                String input = Console.ReadLine();
                open = master.processCommand(input);
            }
        }

        public PuppetMaster(string processURL) {
            _masterURL = processURL;
            _brokers = new List<Broker.Broker>();
            _publishers = new List<Publisher.Publisher>();
            _subscribers = new List<Subscriber.Subscriber>();

            logFilePipe = new StreamWriter(logFile);
        }

        public PuppetMaster(string processURL, String configFilePath) {
            _masterURL = processURL;
            _brokers = new List<Broker.Broker>();
            _publishers = new List<Publisher.Publisher>();
            _subscribers = new List<Subscriber.Subscriber>();

            logFilePipe = new StreamWriter(logFile);

            StreamReader configStream = new StreamReader(configFilePath);
            String inString;

            while ((inString = configStream.ReadLine()) != null) {
                this.processCommand(inString);
            }
            configStream.Close();
        }

        private element findElement(element tree, string Site) {
            element answer;
            //initial case
            if (tree.getSite().Equals(Site)) {
                Console.WriteLine("Found node " + Site);
                return tree;
            }
            //if no children
            else if (tree.getChilds().Count == 0) {
                if (tree.getSite().Equals(Site)) {
                    Console.WriteLine("Found node " + Site);
                    return tree;
                } else {
                    Console.WriteLine("!FOUND " + Site);
                    return null;
                }
            }
            //if has children
            foreach (element e in tree.getChilds()) {
                answer = this.findElement(e, Site);
                if (answer != null) {
                    Console.WriteLine("Found node " + Site);
                    return answer;
                }
            }
            Console.WriteLine("!FOUND " + Site);
            return null;
        }

        private void addElement(String site, string parentSite) {
            element parent;
            if (parentSite.Equals("none") && networkTree == null) {
                networkTree = new element(site, null);
                //writeToLog(site + " is root Root site");
            } else if ((parent = this.findElement(networkTree, parentSite)) != null) {
                parent.addChild(new element(site, parent));
                //writeToLog(site + " created - parent site: " + parentSite);
            }
        }
        
        public void runCommandOnNode(string nodeName, string comand) {
            throw new NotImplementedException();
        }

        public void announcePuppetMaster() {
            int port = Int32.Parse(_masterURL.Split(':')[2].Split('/')[0]);
            string uri = _masterURL.Split(':')[2].Split('/')[1];

            Console.WriteLine("Publishing IPuppetMaster on port: " + port.ToString() + " with uri: " + uri);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, uri, typeof(IPuppetMaster));
        }

        public void connectToNode(string type, string name) {
            Node target = null;
            bool fail = false;
            foreach (Broker.Broker br in _brokers) {
                if (br.getProcessName() == name) {
                    target = br;
                }
            }
            foreach (Subscriber.Subscriber sr in _subscribers) {
                if (sr.getProcessName() == name) {
                    target = sr;
                }
            }
            foreach (Publisher.Publisher pr in _publishers) {
                if (pr.getProcessName() == name) {
                    target = pr;
                }
            }

            switch (type) {
                case "broker":
                    _remoteBroker = (Broker.Broker)Activator.GetObject(typeof(Broker.Broker), target.getProcessURL());
                    if (_remoteBroker == null) {
                        fail = true;
                    }
                    break;
                case "subscriber":
                    _remoteSub = (Subscriber.Subscriber)Activator.GetObject(typeof(Subscriber.Subscriber), target.getProcessURL());
                    if (_remoteSub == null) {
                        fail = true;
                    }
                    break;
                case "publisher":
                    _remotePub = (Publisher.Publisher)Activator.GetObject(typeof(Publisher.Publisher), target.getProcessURL());
                    if (_remotePub == null) {
                        fail = true;
                    }
                    break;
            }
            
            if (fail) {
                Console.WriteLine("Could not extablish Proxy to " + target.getProcessURL());
            }
            else {
                Console.WriteLine("Established Proxy to: " + target.getProcessURL());
            }
        }

        public bool processCommand(String command) {
            String sitePatern = "^Site\\s[A-Za-z0-9]+\\sParent\\s[A-Za-z0-9]+$";
            String processPatern = "^Process\\s[A-Za-z0-9]+\\sIs\\s(broker|publisher|subscriber)\\sOn\\s[A-Za-z0-9]+\\sURL\\stcp://((([0-9]+\\.){3}[0-9])|localhost):[0-9]{3,}/[A-Za-z]+$";
            String routingPatern = "^RoutingPolicy\\s(flooding|filter)$";
            String orderingPatern = "^Ordering\\s(NO|FIFO|TOTAL)$";
            String subPatern = "^Subscriber\\s[A-Za-z0-9]+\\sSubscribe\\s[A-Za-z0-9/\\*]+$";
            String unSubPatern = "^Subscriber\\s[A-Za-z0-9]+\\sUnsubscribe\\s[A-Za-z0-9/\\*]+$";
            String publisherPatern = "^Publisher\\s[A-Za-z0-9]+\\sPublish\\s[0-9]+\\sOnTopic\\s[A-Za-z0-9/]+\\sInterval\\s[0-9]+$";
            String statusPatern = "^Status$";
            String carshPatern = "^Crash\\s[A-Za-z0-9]+$";
            String freezePatern = "^Freeze\\s[A-Za-z0-9]+$";
            String unfreezePatern = "^Unfreeze\\s[A-Za-z0-9]+$";
            String waitPatern = "^Wait\\s[0-9]+$";
            String loggingPatern = "^LoggingLevel\\s(full|light)$";
            String validateWindowsPath = "(?:[\\w]\\:|\\\\|\\.|\\.\\.)(\\\\[A-Za-z_\\-\\s0-9\\.]+)+\\.(txt|log)";
            String importFile = "^Import\\s" + validateWindowsPath + "$";
            String importScript = "^RunScript\\s" + validateWindowsPath + "$";
            String changeLogPath = "^LogFile\\s" + validateWindowsPath + "$";
            String startNetwork = "^StartNetwork$";
            String startProcess = "^Start\\s(broker|subscriber|publisher)\\s[A-Za-z0-9]+$";
            String showPatern = "^Show$";
            String showNodePatern = "^ShowNode\\s(broker|subscriber|publisher)\\s[A-Za-z0-9]+$";
            String SpawnPublicationPatern = "^SpawnPublication\\s[A-Za-z0-9]+\\s[A-Za-z0-9/]+\\s[0-9]+$";
            String addTopicPatern = "^AddTopic\\s[A-Za-z0-9]+\\s[A-Za-z0-9]+\\s[A-Za-z0-9/]+$";
            String quitPatern = "^Quit|Exit$";

            List<Regex> regs = new List<Regex>();
            Match m;
            ArrayList parse = new ArrayList();

            regs.Add(new Regex(sitePatern, RegexOptions.None));
            regs.Add(new Regex(processPatern, RegexOptions.None));
            regs.Add(new Regex(routingPatern, RegexOptions.None));
            regs.Add(new Regex(orderingPatern, RegexOptions.None));
            regs.Add(new Regex(subPatern, RegexOptions.None));
            regs.Add(new Regex(unSubPatern, RegexOptions.None));
            regs.Add(new Regex(publisherPatern, RegexOptions.None));
            regs.Add(new Regex(statusPatern, RegexOptions.None));
            regs.Add(new Regex(carshPatern, RegexOptions.None));
            regs.Add(new Regex(freezePatern, RegexOptions.None));
            regs.Add(new Regex(unfreezePatern, RegexOptions.None));
            regs.Add(new Regex(waitPatern, RegexOptions.None));
            regs.Add(new Regex(loggingPatern, RegexOptions.None));
            regs.Add(new Regex(showPatern, RegexOptions.None));
            regs.Add(new Regex(showNodePatern, RegexOptions.None));
            regs.Add(new Regex(importFile, RegexOptions.None));
            regs.Add(new Regex(importScript, RegexOptions.None));
            regs.Add(new Regex(changeLogPath, RegexOptions.None));
            regs.Add(new Regex(startNetwork, RegexOptions.None));
            regs.Add(new Regex(startProcess, RegexOptions.None));
            regs.Add(new Regex(SpawnPublicationPatern, RegexOptions.None));
            regs.Add(new Regex(addTopicPatern, RegexOptions.None));
            regs.Add(new Regex(quitPatern, RegexOptions.None));

            foreach (Regex r in regs) {
                //Console.WriteLine("Atempting rule: " + r.ToString());
                m = r.Match(command);
                if (m.Success) {
                    //Console.WriteLine("Command Matched to: " + r.ToString());
                    parse = new ArrayList(command.Split(' '));
                    break;
                }
            }
            
            string[] parsed = Array.ConvertAll<object, string>(parse.ToArray(), System.Convert.ToString);
            if (parse.Count > 0) {
                switch (parsed[0]) {
                    case "Site":
                        this.addElement(parsed[1], parsed[3]);
                        break;
                    case "Process":
                        this.createProcess(parsed[1], parsed[3], parsed[5], parsed[7]);
                        break;
                    case "RoutingPolicy":
                        this.changeRoutingLevel(parsed[1]);
                        break;
                    case "Ordering":
                        this.changeOrderLevel(parsed[1]);
                        break;
                    case "Subscriber":
                        if (parsed[2].Equals("Subscribe")) {
                            this.Subscribe(parsed[1], parsed[3]);
                        } else {
                            this.UnSubscribe(parsed[1], parsed[3]);
                        }
                        this.writeToLog(command);
                        break;
                    case "Publisher":
                        this.Publish(parsed[1], Int32.Parse(parsed[3]), parsed[5], Int32.Parse(parsed[7]));
                        this.writeToLog(command);
                        break;
                    case "Status":
                        this.Status();
                        break;
                    case "Import":
                        this.importConfig(parsed[1]);
                        break;
                    case "Crash":
                        this.crash(parsed[1]);
                        this.writeToLog(command);
                        break;
                    case "Freeze":
                        this.freeze(parsed[1]);
                        this.writeToLog(command);
                        break;
                    case "Unfreeze":
                        this.unFreeze(parsed[1]);
                        this.writeToLog(command);
                        break;
                    case "Wait":
                        this.wait(Int32.Parse(parsed[1]));
                        this.writeToLog(command);
                        break;
                    case "LoggingLevel":
                        this.changeLoggingLevel(parsed[1]);
                        break;
                    case "RunScript":
                        this.runScript(parsed[1]);
                        break;
                    case "LogFile":
                        this.changeLogFile(parsed[1]);
                        break;
                    case "Show":
                        this.printSiteTree(networkTree);
                        break;
                    case "ShowNode":
                        this.connectToNode(parsed[1], parsed[2]);
                        break;
                    case "StartNetwork":
                        this.startNetwork();
                        break;
                    case "Start":
                        this.startProcess(parsed[1], parsed[2]);
                        break;
                    case "SpawnPublication":
                        this.spawnPublication(parsed[1], parsed[2], Int32.Parse(parsed[3]));
                        break;
                    case "AddTopic":
                        this.addTopic(parsed[1], parsed[2], parsed[3]);
                        break;
                    case "Exit":
                        wipeNetwork();
                        logFilePipe.Close();
                        return false;
                    case "Quit":
                        logFilePipe.Close();
                        wipeNetwork();
                        return false;
                }
            }
            else {
                Console.Write("Command: \"" + command + "\"" + " is not a recognized command...\n");
            }
            return true;
        }

        private void runScript(string scriptFilePath) {
            StreamReader scriptStream = new StreamReader(scriptFilePath);
            String inString;

            while ((inString = scriptStream.ReadLine()) != null) {
                if (inString.Equals("---EOS---")) {
                    scriptStream.Close();
                    return;
                }
                this.processCommand(inString);
            }
            scriptStream.Close();
        }

        public void importConfig(String configFilePath) {
            StreamReader configStream = new StreamReader(configFilePath);
            String inString;
            this.wipeNetwork();

            while ((inString = configStream.ReadLine()) != null)
            {
                if (inString.Equals("---EOC---")) {
                    configStream.Close();
                    return;
                }
                this.processCommand(inString);
            }
            configStream.Close();
        }
        public void printSiteTree(element tree) {
            tree.printElement();
        }
        public string showSiteTree(element tree) {
            return tree.showElement();
        }
        public void wipeNetwork() {
            this.networkTree = null;
            foreach (Broker.Broker n in _brokers) {
                if (n.getExecuting()) {
                    n.closeProcess();
                }
            }
            _brokers.Clear();
            foreach (Publisher.Publisher n in _publishers) {
                if (n.getExecuting()) {
                    n.closeProcess();
                }
            }
            _publishers.Clear();
            foreach (Subscriber.Subscriber n in _subscribers) {
                if (n.getExecuting()) {
                    n.closeProcess();
                }
            }
            _subscribers.Clear();
        }
        public void createProcess(String processName, String type, String Site, String Url) {
            element targetSite = this.findElement(networkTree, Site);
            Broker.Broker b;
            Publisher.Publisher p;
            Subscriber.Subscriber s;

            string brokerUrl = "";

            if (targetSite == null) {
                Console.WriteLine("No target site found...");
            } else {
                switch (type) {
                    case "broker":
                        if (_routingPolicy) {
                            b = new Broker.Broker(processName, Url, Site, "filter", this._masterURL, _loggingLevel);
                        } else {
                            b = new Broker.Broker(processName, Url, Site, "flooding", this._masterURL, _loggingLevel);
                        }
                        if (targetSite.getParent() != null) {
                            foreach(string url in targetSite.getParent().getBrokerUrls()) {
                                b.addParentUrl(url);
                            }
                            foreach (Broker.Broker parentBroker in targetSite.getParent().getBrokers()) {
                                parentBroker.addChildUrl(Url);
                            }
                        }
                        Console.WriteLine(b.getProcessName());
                        _brokers.Add(b);
                        targetSite.addBroker(b);
                        //writeToLog("Broker " + processName + " created on " + Site + " with process URL " + Url);
                        break;
                    case "publisher":
                        p = new Publisher.Publisher(processName, Url, Site, this._masterURL);
                        foreach (Broker.Broker sb in targetSite.getBrokers()) {
                            p.addBrokerURL(sb.getProcessURL());
                        }
                        _publishers.Add(p);
                        targetSite.addPublisher(p);
                        //writeToLog("Publisher " + processName + " created on " + Site + " with process URL " + Url);
                        break;
                    case "subscriber":
                        brokerUrl = targetSite.getBrokerUrls().ElementAt(0);
                        s = new Subscriber.Subscriber(processName, Url, Site, this._masterURL, _ordering);
                        foreach (Broker.Broker pb in targetSite.getBrokers()) {
                            s.addBrokerURL(pb.getProcessURL());
                        }
                        _subscribers.Add(s);
                        targetSite.addSubscriber(s);
                        foreach (Broker.Broker broker in targetSite.getBrokers()) {
                            broker.addSubscriberUrl(Url);
                        }
                        //writeToLog("Subscriber " + processName + " created on " + Site + " with process URL " + Url);
                        break;
                }
            }
        }

        private void writeToLog(string msg) {
            lock (_logLock) {
                logFilePipe.WriteLine("[" + DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss") + "] - " + msg);
            }
        }

        private void startNetwork() {
            this.announcePuppetMaster();

            foreach (Broker.Broker b in _brokers) {
                startProcess("broker", b.getProcessName());
            }
            foreach (Publisher.Publisher p in _publishers) {
                startProcess("publisher", p.getProcessName());
            }
            foreach (Subscriber.Subscriber s in _subscribers) {
                startProcess("subscriber", s.getProcessName());
            }
        }

        private void startProcess(string type, string name) {
            Node target = null;
            bool found = false;

            switch (type) {
                case "broker":
                    foreach (Broker.Broker b in _brokers) {
                        if (b.getProcessName().Equals(name)) {
                            target = b;
                            found = true;
                            break;
                        }
                    }
                    break;
                case "publisher":
                    foreach (Publisher.Publisher p in _publishers) {
                        if (p.getProcessName() == name) {
                            target = p;
                            found = true;
                            break;
                        }
                    }
                    break;
                case "subscriber":
                    foreach (Subscriber.Subscriber s in _subscribers) {
                        if (s.getProcessName() == name) {
                            target = s;
                            found = true;
                            break;
                        }
                    }
                    break;
            }

            if (found) {
                target.executeProcess();
                //writeToLog("Process " + name + " started");
                Console.WriteLine("Process " + name + " started");
            } else {
                //writeToLog("Process " + name + " cannot be started - Non-Existant");
                Console.WriteLine("Process " + name + " cannot be started - Non-Existant");
            }
        }

        public void spawnPublication(string processName, string topic, int sequenceNumber) {
            foreach (Broker.Broker b in _brokers) {
                if (b.getProcessName().Equals(processName) && b.getExecuting()) {
                    connectToNode("broker", processName);
                    Message msg = new Message(MessageType.Publication, b.getSite(), topic, "demoContent", DateTime.Now, sequenceNumber, "puppetMaster");
                    msg.originURL = b.getProcessURL();
                    _remoteBroker.addToQueue(msg);
                    return;
                }
            }
        }

        public void addTopic(string targetBroker, string interestedNode, string topic) {
            string interestURL = null;

            foreach (Broker.Broker b in _brokers) {
                if (b.getProcessName().Equals(interestedNode)) {
                    interestURL = b.getProcessURL();
                }
            }
            foreach (Subscriber.Subscriber s in _subscribers) {
                if (s.getProcessName().Equals(interestedNode)) {
                    interestURL = s.getProcessURL();
                }
            }

            Console.WriteLine("Adding topic: " + topic + " to " + targetBroker + " for interested " + interestedNode + " of URL " + interestURL);

            foreach (Broker.Broker b in _brokers) {
                if (b.getProcessName().Equals(targetBroker) && b.getExecuting()) {
                    this.connectToNode("broker", targetBroker);
                    _remoteBroker.addTopic(topic, interestURL);
                    Console.WriteLine("Manualy added topic to: " + _remoteBroker.getProcessName());
                }
            }
        }

        public void changeRoutingLevel(string level) {
            bool policy = true;
            if (level.Equals("flooding")) {
                policy = false; 
            }

            _routingPolicy = policy;

            foreach (Broker.Broker b in _brokers) {
                if (b.getExecuting()) {
                    this.connectToNode("broker", b.getProcessName());
                    _remoteBroker.setRoutingPolicy(policy);
                }
                b.setRoutingPolicy(policy);
            }

        }

        public void changeOrderLevel(string ordering) {
            _ordering = ordering;
            foreach (Subscriber.Subscriber s in _subscribers) {
                if (s.getExecuting()) {
                    this.connectToNode("subscriber", s.getProcessName());
                    _remoteSub.setOrdering(ordering);
                }
                s.setOrdering(ordering);
            }
        }

        public void Subscribe(String processName, String topicName) {
            foreach (Subscriber.Subscriber s in _subscribers) {
                if (s.getExecuting() && s.getProcessName().Equals(processName)) {
                    this.connectToNode("subscriber", processName);
                    _remoteSub.subscribe(topicName);
                }
            }
        }
        public void UnSubscribe(String processName, String topicName) {
            foreach (Subscriber.Subscriber s in _subscribers) {
                if (s.getExecuting() && s.getProcessName().Equals(processName)) {
                    this.connectToNode("subscriber", processName);
                    _remoteSub.unsubscribe(topicName);
                }
            }
        }
        public void Publish(String processName, int numberOfEvents, String topic, int intervalMS) {
            foreach (Publisher.Publisher p in _publishers) {
                if (p.getExecuting() && p.getProcessName().Equals(processName)) {
                    Console.WriteLine("Requesting publication at " + processName + " " + numberOfEvents + " times, on topic: \"" + topic + "\" every: " + intervalMS + "ms");
                    this.connectToNode("publisher", processName);
                    _remotePub.addPublishRequest(topic, numberOfEvents, intervalMS);
                }
            }
        }

        public void changeLogFile(String logfilePath) {
            this.logFile = logfilePath;
            logFilePipe.Close();
            logFilePipe = new StreamWriter(this.logFile);
        }
        public void Status() {
            Console.WriteLine("Status request");
            foreach (Broker.Broker b in _brokers) {
                if (b.getExecuting()) {
                    this.connectToNode("broker", b.getProcessName());
                    Console.WriteLine(_remoteBroker.showNode());
                }
            }
            foreach (Subscriber.Subscriber s in _subscribers) {
                if (s.getExecuting()) {
                    this.connectToNode("subscriber", s.getProcessName());
                    Console.WriteLine(_remoteSub.showNode());
                }
            }
            foreach (Publisher.Publisher p in _publishers) {
                if (p.getExecuting()) {
                    this.connectToNode("publisher", p.getProcessName());
                    Console.WriteLine(_remotePub.showNode());
                }
            }

        }
        public void freeze(String processName) {
            Console.WriteLine("Freeze Request");
            foreach (Broker.Broker b in _brokers) {
                if (b.getProcessName().Equals(processName) && b.getExecuting()) {
                    this.connectToNode("broker", b.getProcessName());
                    _remoteBroker.setEnable(false);
                    return;
                }
            }
            foreach (Subscriber.Subscriber s in _subscribers) {
                if (s.getProcessName().Equals(processName) && s.getExecuting()) {
                    this.connectToNode("subscriber", s.getProcessName());
                    _remoteSub.setEnable(false);
                    return;
                }
            }
            foreach (Publisher.Publisher p in _publishers) {
                if (p.getProcessName().Equals(processName) && p.getExecuting()) {
                    this.connectToNode("publisher", p.getProcessName());
                    _remotePub.setEnable(false);
                    return;
                }
            }
        }
        public void unFreeze(String processName) {
            Console.WriteLine("UnFreeze Request");
            foreach (Broker.Broker b in _brokers) {
                if (b.getProcessName().Equals(processName) && b.getExecuting()) {
                    this.connectToNode("broker", b.getProcessName());
                    _remoteBroker.setEnable(true);
                    return;
                }
            }
            foreach (Subscriber.Subscriber s in _subscribers) {
                if (s.getProcessName().Equals(processName) && s.getExecuting()) {
                    this.connectToNode("subscriber", s.getProcessName());
                    _remoteSub.setEnable(true);
                    return;
                }
            }
            foreach (Publisher.Publisher p in _publishers) {
                if (p.getProcessName().Equals(processName) && p.getExecuting()) {
                    this.connectToNode("publisher", p.getProcessName());
                    _remotePub.setEnable(true);
                    return;
                }
            }
        }
        public void wait(int time) {
            Console.WriteLine("Wait Request");
            System.Threading.Thread.Sleep(time);
        }
        public void crash(String processName) {
            foreach (Broker.Broker b in _brokers) {
                if (b.getProcessName().Equals(processName)) {
                    b.closeProcess();
                    return;
                }
            }
            foreach (Publisher.Publisher p in _publishers) {
                if (p.getProcessName().Equals(processName)) {
                    p.closeProcess();
                    return;
                }
            }
            foreach (Subscriber.Subscriber s in _subscribers) {
                if (s.getProcessName().Equals(processName)) {
                    s.closeProcess();
                    return;
                }
            }
        }
        public void changeLoggingLevel(String level) {
            Console.WriteLine("Logging Request: " + level);
            _loggingLevel = level;
            foreach(Broker.Broker b in _brokers) {
                b.setLoggingLevel(level);
                if (b.getExecuting()) {
                    this.connectToNode("broker", b.getProcessName());
                    _remoteBroker.setLoggingLevel(level);
                }
            }
            foreach (Publisher.Publisher p in _publishers) {
                p.setLoggingLevel(level);
                if (p.getExecuting()) {
                    this.connectToNode("publisher", p.getProcessName());
                    _remotePub.setLoggingLevel(level);
                }
            }
            foreach (Subscriber.Subscriber s in _subscribers) {
                s.setLoggingLevel(level);
                if (s.getExecuting()) {
                    this.connectToNode("subscriber", s.getProcessName());
                    _remoteSub.setLoggingLevel(level);
                }
            }
        }

        public void reportToLog(string message) {
            lock (_logLock) {
                this.writeToLog(message);
            }
        }
    }
}
