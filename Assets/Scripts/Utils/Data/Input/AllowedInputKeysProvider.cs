using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Utils.Data.Input
{
    public static class AllowedInputKeysProvider
    {
        private static AllowedInputKeys _instance;

        public static AllowedInputKeys Get()
        {
            if (_instance != null)
                return _instance;


            _instance =
                Addressables.LoadAssetAsync<AllowedInputKeys>(
                        "AllowedInputKeys")
                    .WaitForCompletion();


            return _instance;
        }
        
        private static AllowedInputKeys LoadAllowedInputKeys()
        {
            var guids = AssetDatabase.FindAssets("t:AllowedInputKeys");

            if (guids.Length != 0)
                return AssetDatabase.LoadAssetAtPath<AllowedInputKeys>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
            Debug.LogError("Could not find AllowedInputKeys asset.");
            return null;

        }
    }
}