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
        public string Site { get; }
        public string Topic { get; }
        public string Content { get; }
        public DateTime Timestamp { get; }
        public int Sequence { get; }

        public Message(MessageType subType, string site, string topic, string content, DateTime date, int sequence) {
            this.SubType = subType;
            this.Site = site;
            this.Topic = topic;
            this.Content = content;
            this.Timestamp = date;
            this.Sequence = sequence;
        }

        public override string ToString() {
            string textOutput = "[ " + Timestamp.ToString("dd/MM/yyyy - HH:mm:ss") + " | "+ Sequence + " ] - " +  SubType.ToString() + " of content \"" + Content + "\" with topic " + Topic;
            return textOutput;
        }
    }
}
