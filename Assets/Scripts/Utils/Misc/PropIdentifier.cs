using UnityEngine;

namespace Utils.Misc
{
    public class PropIdentifier : MonoBehaviour
    {
        [SerializeField]
        private string id;


        public string Id => id;


        public void SetId(string value)
        {
            id = value;
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            id ??= "";
        }
#endif
    }
}