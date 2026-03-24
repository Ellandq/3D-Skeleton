namespace Utils.Contract
{
    public interface IUIStackable
    {
        void OnPush();
        void OnPushOther();
        void OnPop();
        void OnPopOther();
    }
}