using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDADLib {
    public class Publication {
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
        }

        public override string ToString() {
            return "Publication";
        }
    }
}
