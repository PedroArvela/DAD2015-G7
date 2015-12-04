using System;

namespace SESDADLib {
    public enum MessageType {
        Publication,
        Subscribe,
        Unsubscribe
    };

    [Serializable]
    public class Message {
        public MessageType SubType { get; }
        public string Sender { get; set; }
        public string SenderSite { get; set; }

        // Total Message Ordering
        public bool Ordered;
        public int Order;
        public string OrderingBroker;

        // FIFO ordering
        public int Sequence { get; }

        public string Site { get; }
        public string Topic { get; }
        public string Publisher { get; }

        public Message(MessageType subType, string site, string publisher, string topic, int sequence) {
            SubType = subType;
            Site = site;
            Topic = topic;
            Sequence = sequence;
            Publisher = publisher;

            Ordered = false;
            Order = -1;
            OrderingBroker = "";
        }

        public override string ToString() {
            string textOutput = "[ " + Sequence + " ] - " + SubType.ToString() + " with topic " + Topic;
            return textOutput;
        }
    }
}
