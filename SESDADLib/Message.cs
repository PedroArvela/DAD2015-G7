using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDADLib {
    public enum MessageType {
        Publication,
        Subscribe,
        Unsubscribe
    };

    [Serializable]
    public class Message {
        public MessageType SubType { get; }
        public string _site;
        public string Topic { get; }
        public string _subject;
        public string _content;
        public DateTime _timestamp;

        public Message(MessageType subType, string site, string topic, string subject, string content, DateTime date) {
            this.SubType = subType;
            this._site = site;
            this.Topic = topic;
            this._subject = subject;
            this._content = content;
            this._timestamp = date;
        }
    }
}
