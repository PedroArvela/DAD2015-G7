using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
        private Dictionary<String, String> siteMap = new Dictionary<string, string>();
        private ArrayList brokers = new ArrayList();
        private ArrayList subscribers = new ArrayList();
        private ArrayList publishers = new ArrayList();

        private bool loggingLevel = false;
        private String logFile = ".\\Logfile.txt";
        private String routingLevel = "Flooding";
        private String orderLevel = "NO";

        static void Main(string[] args) {
            //TODO: something
            PuppetMaster master = new PuppetMaster();
            Console.WriteLine("Welcome!");
            while (true) {
                Console.Write("#: ");
                String input = Console.ReadLine();
                master.processCommand(input);
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

        public void processCommand(String command) {
            String sitePatern = "^Site\\s[A-Za-z0-9]+\\sParent\\s[A-Za-z0-9]+$";
            String processPatern = "^Process\\s[A-Za-z0-9]+\\sIS\\s(broker|publisher|subscriber)\\sOn\\s[A-Za-z0-9]+\\sURL\\stcp://([0-9]+\\.){3}[0-9]:[0-9]{4}/[A-Za-z]+$";
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
                        siteMap.Add(parsed[1], parsed[3]);
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
                }
            }
            else {
                throw new Exception();
            }
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
        public void showSiteTree() {
            foreach (String s in siteMap.Keys) {
                Console.WriteLine("Site: " + s + " --- Parent: " + siteMap[s]);
            }
        }
        public void wipeNetwork() {
            //TODO: something
        }
        public void createProcess(String processName, String type, String Site, String Url) {
            switch (type) {
                case "broker":
                    brokers.Add(new Broker.Broker(Url, Url, Url, 0, routingLevel, true));
                    break;
                case "publisher":
                    publishers.Add(new Publisher.Publisher());
                    break;
                case "subscriber":
                    subscribers.Add(new Subscriber.Subscriber());
                    break;
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
            //TODO: something
        }
        public void crash() {
            //TODO: something
        }
        public void freeze(String processName) {
           //TODO: something
        }
        public void unFreeze(String processName) {
            //TODO: something
        }
        public void wait(int time) {
            System.Threading.Thread.Sleep(time);
        }
        public void crash(String processName) {
            //TODO: something
        }
        public void logginLevel(String level) {
            //TODO: something
        }
    }
}
