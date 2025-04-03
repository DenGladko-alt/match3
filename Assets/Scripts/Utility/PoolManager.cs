using System.Collections.Generic;
using Match3;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] private bool showDebug = false;
    
    private Dictionary<PoolType, Queue<(GameObject, Component)>> poolWithComponents =
        new Dictionary<PoolType, Queue<(GameObject, Component)>>();
    
    private Dictionary<PoolType, GameObject> prefabMapping = new Dictionary<PoolType, GameObject>();

    public void CreatePool<T>(IPoolable poolable, GameObject prefab, int initialSize) where T : Component
    {
        if (!poolWithComponents.ContainsKey(poolable.GetPoolType))
        {
            // Create a new queue for this pool type
            poolWithComponents[poolable.GetPoolType] = new Queue<(GameObject, Component)>();

            // Add the prefab to prefabMapping for later use
            if (!prefabMapping.ContainsKey(poolable.GetPoolType))
            {
                prefabMapping[poolable.GetPoolType] = prefab;
            }

            // Fill the pool with `initialSize` objects
            for (int i = 0; i < initialSize; i++)
            {
                GameObject instance = Instantiate(prefab);
                instance.SetActive(false); // Deactivate the prefab
                var component = instance.GetComponent<T>();

                // Add to the pool
                poolWithComponents[poolable.GetPoolType].Enqueue((instance, component));
            }

            if (showDebug) Debug.Log($"Created pool for {poolable.GetPoolType} with {initialSize} objects.");
        }
        else
        {
            if (showDebug) Debug.LogWarning($"Pool for {poolable.GetPoolType} already exists!");
        }
    }

    public T Spawn<T>(IPoolable poolable, Vector3 position, Quaternion rotation) where T : Component
    {
        if (poolWithComponents.ContainsKey(poolable.GetPoolType))
        {
            if (poolWithComponents[poolable.GetPoolType].Count > 0)
            {
                // Dequeue an object from the pool
                var (obj, component) = poolWithComponents[poolable.GetPoolType].Dequeue();
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);

                // Call OnSpawn for initialization
                var poolableComponent = component as IPoolable;
                poolableComponent?.OnSpawn();

                return (T)component;
            }
            else
            {
                if (showDebug) Debug.LogWarning($"Pool for {poolable.GetPoolType} is empty. Instantiating new object...");

                // Dynamically create and cache a new object if pool is empty
                var prefab = FindPrefabByPoolType(poolable.GetPoolType); // Get the prefab
                if (prefab == null)
                {
                    if (showDebug) Debug.LogError($"No prefab found for PoolType {poolable.GetPoolType}!");
                    
                    return null;
                }

                GameObject newInstance = Instantiate(prefab, position, rotation);
                var newComponent = newInstance.GetComponent<T>();

                // Call OnSpawn
                var poolableComponent = newComponent as IPoolable;
                poolableComponent?.OnSpawn();

                // Optionally add the newly instantiated object back to the pool
                poolWithComponents[poolable.GetPoolType].Enqueue((newInstance, newComponent));

                return newComponent;
            }
        }
        else
        {
            if (showDebug) Debug.LogError($"No pool exists for PoolType {poolable.GetPoolType}!");
            return null;
        }
    }
    
    private GameObject FindPrefabByPoolType(PoolType poolType)
    {
        return prefabMapping.GetValueOrDefault(poolType);
    }

    public void Despawn(IPoolable poolable)
    {
        if (poolWithComponents.ContainsKey(poolable.GetPoolType))
        {
            // Call OnDespawn for cleanup
            poolable.OnDespawn();

            // Cast IPoolable to MonoBehaviour to access gameObject
            var monoBehaviour = poolable as MonoBehaviour;
            if (monoBehaviour != null)
            {
                monoBehaviour.gameObject.SetActive(false);
                monoBehaviour.transform.SetParent(transform);

                // Enqueue back to the pool
                poolWithComponents[poolable.GetPoolType].Enqueue((monoBehaviour.gameObject, monoBehaviour));
            }
            else
            {
                if (showDebug) Debug.LogError("The provided IPoolable object is not a MonoBehaviour!");
            }
        }
        else
        {
            if (showDebug) Debug.LogError($"No pool exists for PoolType {poolable.GetPoolType}!");
        }
    }
    
    public void ClearPool(PoolType poolType)
    {
        if (poolWithComponents.ContainsKey(poolType))
        {
            while (poolWithComponents[poolType].Count > 0)
            {
                var (obj, component) = poolWithComponents[poolType].Dequeue();
                Destroy(obj);
            }
            poolWithComponents.Remove(poolType);
        }
    }
}