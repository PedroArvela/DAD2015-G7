using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Element;
using SESDADLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetMaster {
        private List<Broker.Broker> _brokers;
        private List<Publisher.Publisher> _publishers;
        private List<Subscriber.Subscriber> _subscribers;
        private element networkTree = null; //holds tree for elements in network

        private bool loggingLevel = false;
        private String logFile = ".\\Logfile.txt";
        private String routingLevel = "Flooding";
        private String orderLevel = "NO";
        private String _masterURL = "";
        private StreamWriter logFilePipe;

        static void Main(string[] args) {

            //TODO: something
            PuppetMaster master = new PuppetMaster("tcp://1.2.3.4:1234/puppetMaster");
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
                writeToLog(site + " is root Root site");
            } else if ((parent = this.findElement(networkTree, parentSite)) != null){
                parent.addChild(new element(site, parent));
                writeToLog(site + " created - parent site: " + parentSite);
            }
        }

        public bool processCommand(String command) {
            String sitePatern = "^Site\\s[A-Za-z0-9]+\\sParent\\s[A-Za-z0-9]+$";
            String processPatern = "^Process\\s[A-Za-z0-9]+\\sIs\\s(broker|publisher|subscriber)\\sOn\\s[A-Za-z0-9]+\\sURL\\stcp://([0-9]+\\.){3}[0-9]:[0-9]{3,}/[A-Za-z]+$";
            String routingPatern = "^RoutingPolicy(flooding|filter)$";
            String orderingPatern = "^Ordering\\s(NO|FIFO|TOTAL)$";
            String subPatern = "^Subscriber\\s[A-Za-z0-9]+\\sSubscribe\\s[A-Za-z0-9]+$";
            String unSubPatern = "^Subscriber\\s[A-Za-z0-9]+\\sUnsubscribe\\s[A-Za-z0-9]+$";
            String publisherPatern = "^Publisher\\s[A-Za-z0-9]+\\sPublish\\s[0-9]+\\sOntopic\\s[A-Za-z0-9]+\\sInterval\\s[0-9]+$";
            String statusPatern = "^Status$";
            String carshPatern = "^Crash\\s[A-Za-z0-9]+$";
            String freezePatern = "^Freeze\\s[A-Za-z0-9]+$";
            String unfreezePatern = "^Unfreeze\\s[A-Za-z0-9]+$";
            String waitPatern = "^Wait\\s[0-9]+$";
            String loggingPatern = "^LogginLevel\\s(full|light)$";
            String validateWindowsPath = "(?:[\\w]\\:|\\\\|\\.|\\.\\.)(\\\\[A-Za-z_\\-\\s0-9\\.]+)+\\.(txt|log)";
            String importFile = "^Import\\s" + validateWindowsPath + "$";
            String importScript = "^RunScript\\s" + validateWindowsPath + "$";
            String changeLogPath = "^LogFile\\s" + validateWindowsPath + "$";
            String startNetwork = "^StartNetwork$";
            String startProcess = "^Start\\s(broker|subscriber|publisher)\\s[A-Za-z0-9]+$";
            String showPatern = "^Show$";
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
            regs.Add(new Regex(importFile, RegexOptions.None));
            regs.Add(new Regex(importScript, RegexOptions.None));
            regs.Add(new Regex(changeLogPath, RegexOptions.None));
            regs.Add(new Regex(startNetwork, RegexOptions.None));
            regs.Add(new Regex(startProcess, RegexOptions.None));
            regs.Add(new Regex(quitPatern, RegexOptions.None));

            foreach (Regex r in regs) {
                Console.WriteLine("Atempting rule: " + r.ToString());
                m = r.Match(command);
                if (m.Success) {
                    Console.WriteLine("Command Matched to: " + r.ToString());
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
                        routingLevel = parsed[1];
                        break;
                    case "Ordering":
                        orderLevel = parsed[1];
                        break;
                    case "Subscriber":
                        if (parsed[2].Equals("Subscribe")) {
                            this.UnSubscribe(parsed[1], parsed[3]);
                        } else {
                            this.Subscribe(parsed[1], parsed[3]);
                        }
                        break;
                    case "Publisher":
                        this.Publish(parsed[1], Int32.Parse(parsed[3]), parsed[5], Int32.Parse(parsed[7]));
                        break;
                    case "Status":
                        this.Status();
                        break;
                    case "Import":
                        this.importConfig(parsed[1]);
                        break;
                    case "Crash":
                        this.crash(parsed[1]);
                        break;
                    case "Freeze":
                        this.freeze(parsed[1]);
                        break;
                    case "Unfreeze":
                        this.unFreeze(parsed[1]);
                        break;
                    case "Wait":
                        this.wait(Int32.Parse(parsed[1]));
                        break;
                    case "LogginLevel":
                        this.LogLevel(parsed[1]);
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
                    case "StartNetwork":
                        this.startNetwork();
                        break;
                    case "Start":
                        this.startProcess(parsed[1], parsed[2]);
                        break;
                    case "Exit":
                        logFilePipe.Close();
                        return false;
                    case "Quit":
                        logFilePipe.Close();
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
            //TODO: something
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
                        b = new Broker.Broker(processName, Url, Site, "flooding", this._masterURL);
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
                        writeToLog("Broker " + processName + " created on " + Site + " with process URL " + Url);
                        break;
                    case "publisher":
                        p = new Publisher.Publisher(processName, Url, Site, this._masterURL);
                        foreach (Broker.Broker sb in targetSite.getBrokers()) {
                            p.addBrokerURL(sb.getProcessURL());
                        }
                        _publishers.Add(p);
                        targetSite.addPublisher(p);
                        writeToLog("Publisher " + processName + " created on " + Site + " with process URL " + Url);
                        break;
                    case "subscriber":
                        brokerUrl = targetSite.getBrokerUrls().ElementAt(0);
                        s = new Subscriber.Subscriber(processName, Url, Site, this._masterURL);
                        foreach (Broker.Broker pb in targetSite.getBrokers()) {
                            s.addBrokerURL(pb.getProcessURL());
                        }
                        _subscribers.Add(s);
                        targetSite.addSubscriber(s);
                        writeToLog("Subscriber " + processName + " created on " + Site + " with process URL " + Url);
                        break;
                }
            }
        }

        private void writeToLog(string msg) {
            logFilePipe.WriteLine("[" + DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss")  + "] - " + msg);
        }

        private void startNetwork() {
            throw new NotImplementedException();
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
                writeToLog("Process " + name + " started");
                Console.WriteLine("Process " + name + " started");
            } else {
                writeToLog("Process " + name + " cannot be started - Non-Existant");
                Console.WriteLine("Process " + name + " cannot be started - Non-Existant");
            }
        }

        public void Subscribe(String processName, String topicName) {
            //TODO: something
        }
        public void UnSubscribe(String processName, String topicName) {
            //TODO: something
        }
        public void Publish(String processName, int numberOfEvents, String topicName, int intervalMS) {
            //TODO: something
        }
        public void LogLevel(String type) {
            if (type.Equals("full"))
            {
                loggingLevel = true;
            }
            else {
                loggingLevel = false;
            }
        }
        public void changeLogFile(String logfilePath) {
            this.logFile = logfilePath;
            logFilePipe.Close();
            logFilePipe = new StreamWriter(this.logFile);
        }
        public void Status() {
            Console.WriteLine("Status request");
            //TODO: something
        }
        public void freeze(String processName) {
            Console.WriteLine("Freeze Request");
           //TODO: something
        }
        public void unFreeze(String processName) {
            Console.WriteLine("UnFreeze Request");
            //TODO: something
        }
        public void wait(int time) {
            Console.WriteLine("Wait Request");
            System.Threading.Thread.Sleep(time);
        }
        public void crash(String processName) {
            Console.WriteLine("Crash Resquest");
            //TODO: something
        }
        public void logginLevel(String level) {
            Console.WriteLine("Logging Request");
            //TODO: something
        }
    }
}
