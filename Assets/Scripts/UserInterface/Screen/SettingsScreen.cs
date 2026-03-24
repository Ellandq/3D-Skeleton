using UnityEngine;
using Utils.Contract;

namespace UserInterface.Screen
{
    public class SettingsScreen : ScreenBase, IUIStackable
    {
        [Header("Prefabs")] 
        [SerializeField] private GameObject pageButtonPrefab;

        public void OnPush()
        {
            throw new System.NotImplementedException();
        }

        public void OnPushOther()
        {
            throw new System.NotImplementedException();
        }

        public void OnPop()
        {
            throw new System.NotImplementedException();
        }

        public void OnPopOther()
        {
            throw new System.NotImplementedException();
        }
    }
}