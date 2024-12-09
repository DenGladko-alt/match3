using UnityEngine;

namespace Utility
{
    public abstract class MonoBehaviourService<T> : MonoBehaviour where T : Component
    {
        protected virtual void Awake()
        {
            RegisterService();
        }

        private void RegisterService()
        {
            ServiceManager.Instance.Register(this as T);
        }
    }
}