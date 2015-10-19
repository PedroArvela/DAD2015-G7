namespace SESDADLib {
    public interface IBroker {
        void newPublication(Publication pub);
        void sendPublication(Publication pub);
    }
}
