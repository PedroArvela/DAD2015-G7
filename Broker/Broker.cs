using System;
using System.Collections;
using System.Collections.Generic;

namespace Broker {
    public class Broker {
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

        public Broker(string processName, string processURL, string site, string routingtype) {
            _processName = processName;
            _processURL = processURL;
            _site = site;
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
        /*
        public void newPublication(Publication pub) {
            //TODO: something
        }

        public void sendPublication(Publication pub) {
            //TODO: something
        }
        */
    }
}
