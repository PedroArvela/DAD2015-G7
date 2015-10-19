namespace SESDADLib {
    public interface ISubscriber {
        void subscribe(string topic);
        void unsubscribe(string topic);
        void newPublication(Publication pub);
    }
}
