using UnityEngine;

namespace Managers
{
    public abstract class ManagerBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }

                _instance = FindFirstObjectByType<T>();

                if (!_instance)
                {
                    Debug.LogError($"No instance of type: {typeof(T)}");
                }
                
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _instance = this as T;
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}