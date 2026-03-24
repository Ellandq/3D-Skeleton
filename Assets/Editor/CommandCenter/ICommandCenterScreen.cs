using UnityEngine.UIElements;

namespace Editor.CommandCenter
{
    public interface ICommandCenterScreen
    {
        string ScreenName { get; }

        void Initialize(ICommandCenterLogger logger);

        VisualElement CreateContent();
    }
}