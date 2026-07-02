using UnityEngine;
using Utils.Misc;

namespace Editor.CommandCenter.Screens.Modules.SceneAssets
{
    public static class PropIdUtility
    {
        public static string GetOrCreateId(
            GameObject obj)
        {
            var identifier =
                obj.GetComponent<PropIdentifier>();


            if (!identifier)
            {
                identifier =
                    obj.AddComponent<PropIdentifier>();
            }


            if (string.IsNullOrEmpty(identifier.Id))
            {
                identifier.SetId(
                    System.Guid.NewGuid()
                        .ToString());
            }


            return identifier.Id;
        }
    }
}