using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDADLib {
    public class Node : MarshalByRefObject {
        protected string _processName;
        protected string _processURL;
        protected string _site;
        protected string _puppetMasterURL;
        protected bool _enabled = true;

        public Node(string processName, string processURL, string site, string puppetMasterURL) {
            _processName = processName;
            _processURL = processURL;
            _site = site;
        }

        public string getProcessName(){ return _processURL; }
        public string getProcessURL() { return _processURL; }
        public string getSite() { return _site; }
        public bool getEnabled() { return _enabled; }

        public void toogleEnable(bool enb){ _enabled = enb; }
    }
}
