using System;
using System.Collections;
using System.Collections.Generic;
using SESDADLib;

namespace Broker {
    public class Broker : MarshalByRefObject, IBroker {
        private bool _routingPolicy;

        private string _processName;
        private string _processURL;
        private string _site;
        private Dictionary<string, int> _subscribersTopics = new Dictionary<string, int>(); //key == topic, value == #subscribers
        private ArrayList _parentProcessesURL = new ArrayList();
        private ArrayList _childProcessesURL = new ArrayList();
        
        private bool _enabled = true;
        private bool _delayed = false;
        private int _delayTime = 0;

        private string _puppetMasterURL;

        public Broker(string processName, string processURL, string site, string routingtype, string puppetMasterURL) {
            _processName = processName;
            _processURL = processURL;
            _site = site;
            _puppetMasterURL = puppetMasterURL;
            switch (routingtype) {
                case "flooding":
                    _routingPolicy = false;
                    break;
                case "filter":
                    _routingPolicy = true;
                    break;
            }
        }

        public string getSite() { return _site; }
        public string getProcessURL() { return _processURL; }
        public ArrayList getParentURL() { return _parentProcessesURL; }
        public ArrayList getChildURL() { return _childProcessesURL; }

        public void addChildUrl(string url) {
            _childProcessesURL.Add(url);
        }
        public void addParentUrl(string url) {
            _parentProcessesURL.Add(url);
        }

        public bool toggleNode() {
            return (_enabled = !_enabled);
        }

        public bool toggleDelay(int time) {
            _delayTime = time;
            return (_delayed = !_delayed);
        }

        public void printBroker() {
            string print = "\tBroker: " + _processName + "for " + _site + " active on " + _processURL + "\n";
            print += "\tParent Broker URL(s):\n";
            foreach (string purl in _parentProcessesURL) {
                print += "\t\t" + purl + "\n";
            }
            print += "\tChild Broker URL(s):\n";
            foreach (string curl in _childProcessesURL) {
                print += "\t\t" + curl + "\n";
            }
            print += "\tTopic(s):\n";
            foreach (string topic in _subscribersTopics.Keys) {
                print += "\t\t" + topic + "with " + _subscribersTopics[topic].ToString() + "subscribers\n";
            }
            Console.Write(print);
        }

        public void newPublication(Publication pub) {
            throw new NotImplementedException();
        }

        public void sendPublication(Publication pub) {
            throw new NotImplementedException();
        }
    }
}
