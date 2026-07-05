using UnityEngine;

namespace Utils.Misc.Props
{
    public static class PropIdUtility
    {
        public static string GetOrCreateId(GameObject obj, string assetPath)
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
                    System.Guid.NewGuid().ToString(),
                    assetPath);
            }


            return identifier.Id;
        }
    }
}