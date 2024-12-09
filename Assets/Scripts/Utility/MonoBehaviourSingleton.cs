using UnityEngine;

namespace Utility
{
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        Debug.LogError($"An instance of {typeof(T)} not found in the scene.");
                    }
                }
                return instance;
            }
        }
	
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else if (instance != this)
            {
                Debug.LogWarning("Another instance of " + typeof(T) + " exists. Destroying this one.");
                Destroy(gameObject);
            }
        }
    }
}
