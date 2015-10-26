using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDADLib {
    public class Publication {
        public string _site {
            get;
        }
        public string _topic {
            get;
        }
        public string _subject {
            get;
        }
        public string _content {
            get;
        }
        public DateTime _timestamp {
            get;
        }

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
