namespace SESDADLib {
    public interface INode {
        string Url();
        void addToQueue(Message msg);
        string getSite();
    }
}
