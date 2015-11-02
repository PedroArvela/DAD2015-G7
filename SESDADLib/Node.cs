﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDADLib {
    public abstract class Node : MarshalByRefObject {
        protected string _processName;
        protected string _processURL;
        protected string _site;
        protected string _puppetMasterURL;
        protected bool _enabled = true;
        protected Process _nodeProcess;

        public Node(string processName, string processURL, string site, string puppetMasterURL) {
            _processName = processName;
            _processURL = processURL;
            _site = site;
            _nodeProcess = new Process();
        }

        public string getProcessName(){ return _processName; }
        public string getProcessURL() { return _processURL; }
        public string getSite() { return _site; }
        public bool getEnabled() { return _enabled; }

        public void toogleEnable(bool enb){ _enabled = enb; }

        public abstract string showNode();

        public abstract void printNode();

        //Method to run when a command is published by the PuppetMaster
        public abstract void OnRunCommand(string command);

        protected abstract string getArguments();

        public abstract void executeProcess();
    }
}
