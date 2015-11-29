using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDADLib {
    public abstract class Node : MarshalByRefObject {
        protected string _processName;
        protected string _processURL;
        protected int _port;
        protected string _uriAddress;

        protected string _site;

        protected Dictionary<string, INode> _connections; //key == url, value == INode

        protected string _puppetMasterURL;
        protected bool _enabled = true;
        protected bool _executing = false;
        protected string _loggingLevel;
        protected Process _nodeProcess;

        public Node(string processName, string processURL, string site, string puppetMasterURL) {
            _processName = processName;
            _processURL = processURL;
            _site = site;
            _puppetMasterURL = puppetMasterURL;
            _loggingLevel = "light";
            _nodeProcess = new Process();

            _port = int.Parse(_processURL.Split(':')[2].Split('/')[0]);
            _uriAddress = _processURL.Split(':')[2].Split('/')[1];

            _connections = new Dictionary<string, INode>();
        }

        public string Url() { return getProcessURL(); }

        public string getProcessName() { return _processName; }
        public string getProcessURL() { return _processURL; }
        public string getSite() { return _site; }
        public bool getEnabled() { return _enabled; }
        public bool getExecuting() { return _executing; }
        public string getLoggingLevel() { return _loggingLevel; }

        public void setEnable(bool enb) { _enabled = enb; }
        public void setLoggingLevel(string level) { _loggingLevel = level; }

        public abstract string showNode();

        public abstract void printNode();

        public abstract void publishToPuppetMaster();

        protected abstract string getArguments();

        public abstract void executeProcess();

        protected INode aquireConnection(string url) {
            INode target = null;

            _connections.TryGetValue(url, out target);

            if (target == null) {
                target = (INode)Activator.GetObject(typeof(INode), url);
                _connections.Add(url, target);
                return target;
            } else {
                return target;
            }
        }

        public void closeProcess() {
            _nodeProcess.Kill();
            _executing = false;
        }

        public void writeToLog(string message) {
            IPuppetMaster pm = (IPuppetMaster)Activator.GetObject(typeof(IPuppetMaster), _puppetMasterURL);
            pm.reportToLog(message);
        }
    }
}
