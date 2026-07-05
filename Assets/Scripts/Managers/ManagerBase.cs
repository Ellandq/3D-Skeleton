using UnityEngine;

namespace Managers
{
    public abstract class ManagerBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isQuitting;
        
        static ManagerBase()
        {
            Application.quitting += () => _isQuitting = true;
        }

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                    return null;

                if (_instance)
                    return _instance;

                _instance = FindAnyObjectByType<T>();

                if (!_instance && !_isQuitting)
                    Debug.LogError($"No instance of type: {typeof(T)}");

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (!_instance)
            {
                _instance = this as T;
                DontDestroyOnLoad(!transform.parent ? gameObject : transform.parent.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            _instance = null;
        }
    }
}