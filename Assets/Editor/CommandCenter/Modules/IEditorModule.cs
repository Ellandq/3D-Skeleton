using UnityEngine.UIElements;

namespace Editor.CommandCenter.Modules
{
    public interface IEditorModule
    {
        string ModuleName { get; }
        float MinHeight { get; }
        float MaxHeight { get; }

        ModuleStatus Status { get; }

        void Initialize(ICommandCenterLogger logger);

        VisualElement CreateContent();
        void Validate();
        void Enforce();
    }
}