using System;
using UserInterface;

namespace Utils.Contract
{
    public interface IUIStackable
    {
        void OnPush(bool instant, Action onActivate = null);
        void OnPushOther();
        void OnPop(bool instant, Action onDeactivate = null);
        void OnPopOther();
    }
}