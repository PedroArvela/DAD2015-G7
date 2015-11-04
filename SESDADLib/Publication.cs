using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDADLib {
    [Serializable]
    public class Publication {
        private bool _subType;
        public string _site;
        public string _topic;
        public string _subject;
        public string _content;
        public DateTime _timestamp;

        public Publication(string site, string topic, string subject, string content, DateTime date) {
            _site = site;
            _topic = topic;
            _subject = subject;
            _content = content;
            _timestamp = date;
            _subType = false;
        }

        public string getTopic() { return _topic; }
        public bool getSubType() { return _subType; }

        public override string ToString() {
            return "Publication";
        }
    }
}
