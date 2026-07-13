using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Editor.CommandCenter.Screens.Modules.SceneAssets
{
    public static class AddressableLoader
    {
        public static GameObject LoadPrefab(string address)
        {
            if (string.IsNullOrEmpty(address))
                return null;


            var handle =
                Addressables.LoadAssetAsync<GameObject>(address);


            handle.WaitForCompletion();


            if (handle.Result) return handle.Result;
            Debug.LogError(
                $"Failed loading addressable: {address}");

            return null;


        }
    }
}