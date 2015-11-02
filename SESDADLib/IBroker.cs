namespace SESDADLib {
    public interface IBroker {
        void newPublication(Publication pub);
        void sendPublication(Publication pub);
        void subscribe(string topic);
        void unsubscribe(string topic);
    }
}
