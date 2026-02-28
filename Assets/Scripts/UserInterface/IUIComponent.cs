using System.Collections;

namespace UserInterface
{
    public interface IUIComponent
    {
        void Activate(bool instant);
        void Deactivate(bool instant);
    }
}