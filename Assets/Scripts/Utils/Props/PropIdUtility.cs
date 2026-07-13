using Components.Props;
using UnityEngine;

namespace Utils.Props
{
    public static class PropIdUtility
    {
        public static PropIdentifier GetOrCreateId(GameObject obj, string assetPath, string id = "")
        {
            var identifier =
                obj.GetComponent<PropIdentifier>();


            if (!identifier)
            {
                identifier =
                    obj.AddComponent<PropIdentifier>();
            }


            if (string.IsNullOrEmpty(identifier.Id) || !string.IsNullOrEmpty(id))
            {
                identifier.Initialize(
                    string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id,
                    assetPath);
            }


            return identifier;
        }
    }
}