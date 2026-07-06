using System.Collections.Generic;
using System.Linq;
using Model.Enum.Named;
using UnityEngine;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;
using UserInterface.Windows;
using Utils.Enum;

namespace Model.Data.Model
{
    [CreateAssetMenu(menuName = "Scenes/Scene Profile Re")]
    public class SceneProfile : ScriptableObject
    {
        [Header("Scene Settings")]
        public NamedScene sceneName;
        public List<NamedScene> subScenes = new();

        public bool useLoadScreen;
        public bool saveChanges;

        [Header("UI Assets")]
        [SerializeField] private List<NamedHUD> hudKeys = new();
        public List<NamedHUD> HudKeys =>
            hudKeys.Union(_subProfiles.Values.SelectMany(p => p.hudKeys)).ToList();

        [SerializeField] private List<NamedOverlay> overlayKeys = new();
        public List<NamedOverlay> OverlayKeys =>
            overlayKeys.Union(_subProfiles.Values.SelectMany(p => p.overlayKeys)).ToList();

        [SerializeField] private List<NamedScreen> screenKeys = new();
        public List<NamedScreen> ScreenKeys =>
            screenKeys.Union(_subProfiles.Values.SelectMany(p => p.screenKeys)).ToList();

        [SerializeField] private List<NamedWindow> windowKeys = new();
        public List<NamedWindow> WindowKeys =>
            windowKeys.Union(_subProfiles.Values.SelectMany(p => p.windowKeys)).ToList();

        [Header("Props")]
        public SceneAssetData assetData;

        private readonly Dictionary<NamedScene, SceneProfile> _subProfiles = new();

        public void AssignSubSceneProfile(NamedScene scene, SceneProfile profile)
        {
            _subProfiles[scene] = profile;
        }

        private List<SceneProfile> SubProfiles()
            => _subProfiles.Values.ToList();
        
        public List<NamedScene> GetAllScenes()
        {
            return subScenes
                .Prepend(sceneName)
                .Distinct()
                .ToList();
        }
        
        public Dictionary<NamedScene, SceneProfile> GetAllProfiles() 
            => SubProfiles()
                .Prepend(this)
                .ToDictionary(p => p.sceneName, p => p);
    }
}