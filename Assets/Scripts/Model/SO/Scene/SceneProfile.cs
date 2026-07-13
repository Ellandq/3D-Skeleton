using System.Collections.Generic;
using Model.Data.Scene;
using UnityEngine;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;
using UserInterface.Windows;
using Utils.Enum;

namespace Model.SO.Scene
{
    [CreateAssetMenu(menuName = "Scenes/Scene Profile Re")]
    public class SceneProfile : ScriptableObject
    {
        [Header("Scene Settings")]
        public NamedScene sceneName;
        public List<NamedScene> subScenes = new();

        public bool useLoadScreen;
        public bool saveChanges; // TODO Implement

        [Header("UI Assets")]
        public List<NamedHUD> hudKeys = new();
        public List<NamedOverlay> overlayKeys = new();
        public List<NamedScreen> screenKeys = new();
        public List<NamedWindow> windowKeys = new();

        [Header("Props")]
        public SceneAssetData assetData;
    }
}