using UnityEngine.UIElements;

namespace Editor.CommandCenter.Modules
{
    public interface IEditorModule
    {
        string ModuleName { get; }

        ModuleStatus Status { get; }

        void Initialize(ICommandCenterLogger logger);

        VisualElement CreateContent();
        void Validate();
        void Enforce();
    }
}