using System.Collections.Generic;
using System.Linq;
using Model.Data.Registry;
using Model.SO.Scene;
using UnityEngine;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;
using UserInterface.Windows;
using Utils.Enum;
using Utils.Props;

namespace Model.Data.Scene
{
    public class RuntimeSceneProfile
    {
        public NamedScene sceneName;

        private Dictionary<NamedScene, RuntimeSceneProfile> profiles = new();

        public IReadOnlyDictionary<NamedScene, RuntimeSceneProfile> Profiles => profiles;

        private List<NamedScene> subScenes = new();

        public List<NamedScene> SubScenes => subScenes;

        [Header("UI Assets")]
        private List<NamedHUD> hudKeys = new();

        public List<NamedHUD> HudKeys =>
            hudKeys
                .Union(profiles.Values.SelectMany(p => p.hudKeys))
                .ToList();

        private List<NamedOverlay> overlayKeys = new();

        public List<NamedOverlay> OverlayKeys =>
            overlayKeys
                .Union(profiles.Values.SelectMany(p => p.overlayKeys))
                .ToList();

        private List<NamedScreen> screenKeys = new();

        public List<NamedScreen> ScreenKeys =>
            screenKeys
                .Union(profiles.Values.SelectMany(p => p.screenKeys))
                .ToList();

        private List<NamedWindow> windowKeys = new();

        public List<NamedWindow> WindowKeys =>
            windowKeys
                .Union(profiles.Values.SelectMany(p => p.windowKeys))
                .ToList();

        public RuntimeSceneAssetData assetData;

        public bool useLoadScreen;


        public static RuntimeSceneProfile BuildRuntimeProfile(
            SceneProfile baseProfile,
            DiffRegistry diff)
        {
            var runtimeAssetData = new RuntimeSceneAssetData();

            foreach (var runtimeCollection in baseProfile.assetData.collections.Select(source => new PropCollection
                     {
                         assetAddress = source.assetAddress,
                         props = source.props.ToList(),
                         dynamicProps = source.dynamicProps
                             .Select(DynamicPropUtility.Clone)
                             .ToList()
                     }))
            {
                diff.ApplyToCollection(
                    baseProfile.sceneName,
                    runtimeCollection);

                runtimeAssetData.collections.Add(runtimeCollection);
            }

            return new RuntimeSceneProfile
            {
                sceneName = baseProfile.sceneName,
                assetData = runtimeAssetData,

                hudKeys = baseProfile.hudKeys,
                overlayKeys = baseProfile.overlayKeys,
                screenKeys = baseProfile.screenKeys,
                windowKeys = baseProfile.windowKeys,

                useLoadScreen = baseProfile.useLoadScreen,

                subScenes = baseProfile.subScenes
            };
        }


        public void InitializeSubSceneProfiles(
            IEnumerable<RuntimeSceneProfile> subSceneProfiles)
        {
            profiles = subSceneProfiles
                .ToDictionary(
                    p => p.sceneName,
                    p => p);
        }


        public IReadOnlyDictionary<NamedScene, RuntimeSceneProfile> GetAllProfiles()
        {
            var result = new Dictionary<NamedScene, RuntimeSceneProfile>
            {
                [sceneName] = this
            };

            foreach (var profile in profiles)
            {
                result[profile.Key] = profile.Value;
            }

            return result;
        }

        public List<NamedScene> GetAllScenes()
        {
            return new[] { sceneName }
                .Concat(profiles.Keys)
                .ToList();
        }
    }
}