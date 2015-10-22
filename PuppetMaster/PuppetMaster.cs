﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Element;
using Broker;
using Publisher;
using Subscriber;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetMaster {
        //Key = site
        //value = mothersite
        private element networkTree = null; //holds tree for elements in network

        private bool loggingLevel = false;
        private String logFile = ".\\Logfile.txt";
        private String routingLevel = "Flooding";
        private String orderLevel = "NO";

        static void Main(string[] args) {
            //TODO: something
            PuppetMaster master = new PuppetMaster();
            bool open = true;

            Console.WriteLine("Welcome!");
            while (open) {
                Console.Write("#: ");
                String input = Console.ReadLine();
                open = master.processCommand(input);
            } 
        }

        public PuppetMaster() {

        }

        public PuppetMaster(String configFilePath) {
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
            } else if ((parent = this.findElement(networkTree, parentSite)) != null){
                parent.addChild(new element(site, parent));
            }
            this.showSiteTree(networkTree);
        }

        public bool processCommand(String command) {
            String sitePatern = "^Site\\s[A-Za-z0-9]+\\sParent\\s[A-Za-z0-9]+$";
            String processPatern = "^Process\\s[A-Za-z0-9]+\\sIs\\s(broker|publisher|subscriber)\\sOn\\s[A-Za-z0-9]+\\sURL\\stcp://([0-9]+\\.){3}[0-9]:[0-9]{4}/[A-Za-z]+$";
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
            String showPatern = "^Show$";
            String quitPatern = "^Quit|Exit$";

            ArrayList regs = new ArrayList();
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
                        if (parsed[2].Equals("Subscribe"))
                        {
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
                    case "Show":
                        this.showSiteTree(networkTree);
                        break;
                    case "Exit":
                        return false;
                    case "Quit":
                        return false;
                }
            }
            else {
                Console.Write("Command: \"" + command + "\"" + " is not a recognized command...\n");
            }
            return true;
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
        public void showSiteTree(element tree) {
            Console.WriteLine("SHOWING SITE: " + tree.getSite());
            if (tree != null) {
                if (tree.getParent() == null) {
                    Console.WriteLine("\tParent: none");
                } else {
                    Console.WriteLine("\tParent: " + tree.getParent().getSite());
                }
                Console.WriteLine("\tRegistered Brokers: " + tree.getBrokers().Count);
                Console.WriteLine("\tRegistered Subscribers: " + tree.getSubscribers().Count);
                Console.WriteLine("\tRegistered Publishers: " + tree.getPublishers().Count);
                foreach (Broker.Broker b in tree.getBrokers()) {
                    b.printBroker();
                }                
                foreach (element c in tree.getChilds()) {
                    this.showSiteTree(c);
                }
            }
        }
        public void wipeNetwork() {
            //TODO: something
        }
        public void createProcess(String processName, String type, String Site, String Url) {
            element targetSite = this.findElement(networkTree, Site);
            Broker.Broker b;

            Console.WriteLine("Create Process Request");

            if (targetSite == null) {
                Console.WriteLine("No target site found...");
            } else {
                switch (type) {
                    case "broker":
                        b = new Broker.Broker(processName, Url, Site, "flooding");
                        if (targetSite.getParent() != null) {
                            foreach(string url in targetSite.getParent().getBrokerUrls()) {
                                b.addParentUrl(url);
                            }
                            foreach (Broker.Broker parentBroker in targetSite.getParent().getBrokers()) {
                                parentBroker.addChildUrl(Url);
                            }
                        }
                        targetSite.addBroker(b);
                        break;
                    case "publisher":
                        break;
                    case "subscriber":
                        break;
                }
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
