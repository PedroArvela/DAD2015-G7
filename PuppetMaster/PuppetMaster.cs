using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class PuppetMaster {
        //Key = site
        //value = mothersite
        private Dictionary<String, String> siteMap;

        static void Main(string[] args) {
            //TODO: something
            Console.WriteLine("Hello world!");
        }

        public PuppetMaster(String configFilePath) {
            StreamReader configStream = new StreamReader(configFilePath);
            String inString;
            siteMap = new Dictionary<string, string>();


            while ((inString = configStream.ReadLine()) != null) {
                this.processCommand(inString);
            }
            configStream.Close();
        }

        public void processCommand(String command) {
            String sitePatern = "^Site [Aa-Zz0-9]+ Parent (none|[Aa-Zz0-9]+)\b";
            String processPatern = "^Process [Aa-Zz0-9]+ IS (broker|publisher|subscriber) On [Aa-Zz0-9]+ URL tcp://([0-9]+\\.){3}[0-9]:[0-9]{4}/[Aa-Zz]+\b";
            String routingPatern = "^RoutingPolicy(flooding|filter)\b";
            String orderingPatern = "^Ordering (NO|FIFO|TOTAL)";
            String subPatern = "^Subscriber [Aa-Zz0-9]+ Subscribe [Aa-Zz0-9]+\b";
            String unSubPatern = "^Subscriber [Aa-Zz0-9]+ Unsubscribe [Aa-Zz0-9]+\b";
            String publisherPatern = "^Publisher [Aa-Zz0-9]+ Publish [0-9]+ Ontopic [Aa-Zz0-9]+ Interval [0-9]+\b";
            String statusPatern = "^Status\b";
            String carshPatern = "^Crash [Aa-Zz0-9]+\b";
            String freezePatern = "^Freeze [Aa-Zz0-9]+\b";
            String unfreezePatern = "^Unfreeze [Aa-Zz0-9]+\b";
            String waitPatern = "^Wait [0-9]+\b";
            String loggingPatern = "^LogginLevel (full|light)\b";

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
                m = r.Match(command);
                if (m.Success) {
                    parse = new ArrayList(command.Split(' '));
                    break;
                }
            }

            String[] parsed = (String[])parse.ToArray();
            if (parse.Count > 0) {
                switch (parsed[0]) {
                    case "Site":
                        siteMap.Add(parsed[1], parsed[3]);
                        break;
                    case "Process":
                        break;
                    case "RoutingPolicy":
                        break;
                    case "Ordering":
                        break;
                    case "Subscriber":
                        break;
                    case "Publisher":
                        break;
                    case "Status":
                        break;
                    case "Crash":
                        break;
                    case "Freeze":
                        break;
                    case "Unfreeze":
                        break;
                    case "Wait":
                        break;
                    case "LogginLevel":
                        break;
                }
            }
            else {
                throw new Exception();
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
            //TODO: something
        }
        public void Status() {
            //TODO: something
        }
    }
}
