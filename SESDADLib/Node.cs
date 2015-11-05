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

        protected string _puppetMasterURL;
        protected bool _enabled = true;
        protected bool _executing = false;
        protected Process _nodeProcess;

        public Node(string processName, string processURL, string site, string puppetMasterURL) {
            _processName = processName;
            _processURL = processURL;
            _site = site;
            _puppetMasterURL = puppetMasterURL;
            _nodeProcess = new Process();
            
            _port = int.Parse(_processURL.Split(':')[2].Split('/')[0]);
            _uriAddress = _processURL.Split(':')[2].Split('/')[1];
        }

        public string getProcessName(){ return _processName; }
        public string getProcessURL() { return _processURL; }
        public string getSite() { return _site; }
        public bool getEnabled() { return _enabled; }
        public bool getExecuting() { return _executing; }

        public void setEnable(bool enb){ _enabled = enb; }

        public abstract string showNode();

        public abstract void printNode();

        public abstract void publishToPuppetMaster();

        protected abstract string getArguments();

        public abstract void executeProcess();

        public void closeProcess() {
            _nodeProcess.Kill();
            _executing = false;
        }
    }
}
