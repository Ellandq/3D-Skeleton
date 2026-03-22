using UnityEngine.UIElements;

namespace Editor.CommandCenter.Modules.Validation
{
    public interface IEditorValidationModule
    {
        string ModuleName { get; }

        ModuleStatus Status { get; }

        void Initialize(ICommandCenterLogger logger);

        VisualElement CreateContent();
        void Validate();
        void Enforce();
    }
}