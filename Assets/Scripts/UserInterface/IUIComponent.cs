using System;

namespace UserInterface
{
    public interface IUIComponent
    {
        void Activate(bool instant, Action onActivate = null);
        void Deactivate(bool instant, Action onDeactivate = null);
    }
}