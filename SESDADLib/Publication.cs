using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDADLib {
    public class Publication {
        public string site {
            get;
        }
        public string topic {
            get;
        }
        public string subject {
            get;
        }
        public string content {
            get;
        }

        public Publication(string site, string topic, string subject, string content) {
            this.site = site;
            this.topic = topic;
            this.subject = subject;
            this.content = content;
        }
    }
}
