using SESDADLib;
using System;
using System.Collections.Generic;

namespace Publisher{
    public class Publisher{
        static void Main(string[] args) {
            //TODO: something
        }

        private String _name;
        private String _processURL;
        private String _site;
        private List<string> _siteBrokerUrl;

        private string _puppetMasterURL;

        public Publisher(string processName, string processURL, string site, string puppetMasterURL) {
            _name = processName;
            _processURL = processURL;
            _site = site;
            _puppetMasterURL = puppetMasterURL;
            _siteBrokerUrl = new List<string>();
        }

        public void addBrokerURL(string url) {
            _siteBrokerUrl.Add(url);
        }

        public void Publish(Publication pub) {
            //TODO: something
        }
    }

    internal interface IPublisher {
    }
}
