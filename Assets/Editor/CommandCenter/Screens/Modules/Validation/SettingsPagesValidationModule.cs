using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UserInterface.Screen;
using Utils.SO.Settings.Screen;
using Object = UnityEngine.Object;

namespace Editor.CommandCenter.Screens.Modules.Validation
{
    public class SettingsPagesValidationModule : IEditorValidationModule
    {
        public string ModuleName => "Settings Pages";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Unknown;

        private ICommandCenterLogger _logger;

        private const string PagesPath = "Assets/ScriptableObjects/Settings/Pages";

        public void Initialize(ICommandCenterLogger logger)
        {
            _logger = logger;
        }

        public VisualElement CreateContent()
        {
            return new Label("Validates that all SettingsPageSO assets are assigned to SettingsScreen.");
        }

        #region PUBLIC

        public void Validate()
        {
            Status = ModuleStatus.Valid;

            var screen = FindSettingsScreen();
            if (!screen)
            {
                _logger.LogError("SettingsScreen not found in scene.");
                Status = ModuleStatus.Error;
                return;
            }

            var allPages = LoadAllPages();
            var assignedPages = screen.pageAssets ?? new List<SettingsPageSO>();

            foreach (var page in allPages.Where(page => !assignedPages.Contains(page)))
            {
                _logger.LogWarning($"Missing page in SettingsScreen: {page.name}");
                if (Status != ModuleStatus.Error)
                    Status = ModuleStatus.Warning;
            }

            foreach (var page in assignedPages.Where(page => !allPages.Contains(page)))
            {
                _logger.LogWarning($"Extra page assigned (not in folder): {page.name}");
                if (Status != ModuleStatus.Error)
                    Status = ModuleStatus.Warning;
            }
        }

        public void Enforce()
        {
            Status = ModuleStatus.Valid;

            var screen = FindSettingsScreen();
            if (!screen)
            {
                _logger.LogError("SettingsScreen not found in scene.");
                Status = ModuleStatus.Error;
                return;
            }

            var allPages = LoadAllPages()
                .OrderBy(p => p.index)
                .ToList();

            Undo.RecordObject(screen, "Auto-assign Settings Pages");
            screen.pageAssets = allPages;

            EditorUtility.SetDirty(screen);
            AssetDatabase.SaveAssets();

            _logger.Log($"SettingsScreen pageAssets rebuilt with {allPages.Count} entries.");
        }

        #endregion

        #region HELPERS
        
        private static SettingsScreen FindSettingsScreen()
        {
            return Object.FindAnyObjectByType<SettingsScreen>();
        }

        private static List<SettingsPageSO> LoadAllPages()
        {
            var guids = AssetDatabase.FindAssets("t:SettingsPageSO", new[] { PagesPath });

            return guids.Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SettingsPageSO>)
                .Where(asset => asset)
                .ToList();
        }

        #endregion
    }
}