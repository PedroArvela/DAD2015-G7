using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Element
{
    public class element {
        private string _site;
        private List<Broker.Broker> _brokers = new List<Broker.Broker>();
        private List<Subscriber.Subscriber> _subscribers = new List<Subscriber.Subscriber>();
        private List<Publisher.Publisher> _publishers = new List<Publisher.Publisher>();

        private element _parent;
        private List<element> _childs = new List<element>();

        public element(String siteURL, element parent)
        {
            _site = siteURL;
            _parent = parent;
        }

        public string getSite() { return _site; }
        public List<Broker.Broker> getBrokers() { return _brokers; }
        public List<Subscriber.Subscriber> getSubscribers() { return _subscribers; }
        public List<Publisher.Publisher> getPublishers() { return _publishers; }

        public element getParent() { return _parent; }
        public List<element> getChilds() { return _childs; }

        public List<string> getParentUrls()
        {
            List<string> answer = new List<string>();
            List<Broker.Broker> parentBrokers = _parent.getBrokers();

            foreach (Broker.Broker b in parentBrokers)
            {
                foreach (String url in b.getParentURL())
                {
                    answer.Add(url);
                }
            }
            return answer;
        }

        public List<string> getBrokerUrls() {
            List<string> answer = new List<string>();

            foreach (Broker.Broker b in _brokers) {
                answer.Add(b.getProcessURL());
            }
            return answer;
        }

        public void addChild(element c) { _childs.Add(c); }
        public void addBroker(Broker.Broker b) { _brokers.Add(b); }
        public void addSubscriber(Subscriber.Subscriber s) { _subscribers.Add(s); }
        public void addPublisher(Publisher.Publisher p) { _publishers.Add(p); }

        public void printElement() {
            Console.WriteLine(this.showElement());
        }

        public string showElement() {
            string print = "----" + _site + "----\n";
                        
            if (_parent == null) {
                print += "\tParent: none\n";
            } else {
                print += "\tParent: " + _parent.getSite() + "\n";
            }
            print += "\tRegistered Brokers: " + _brokers.Count + "\n";
            print += "\tRegistered Subscribers: " + _subscribers.Count + "\n";
            print += "\tRegistered Publishers: " + _publishers.Count + "\n";
            print += "---Brokers---\n";
            foreach (Broker.Broker b in _brokers) {
                print += b.showNode();
            }
            print += "---Publishers---\n";
            foreach (Publisher.Publisher p in _publishers){
                print += p.showNode();
            }
            print += "---Subscribers---\n";
            foreach (Subscriber.Subscriber s in _subscribers) {
                print += s.showNode();
            }
            foreach (element c in _childs) {
                print += c.showElement();
            }

            return print;
        }
    }
}
