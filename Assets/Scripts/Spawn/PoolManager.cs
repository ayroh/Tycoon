using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pool
{
    public class PoolManager : Singleton<PoolManager>
    {
        [Header("Parents")]
        [SerializeField] private Transform pooledObjectParent;
        [SerializeField] private Transform ingameParent;

        [Header("Serialized Objects")]
        [SerializeField] private PoolObjects poolObjects;

        private readonly Dictionary<PoolObjectType, Pool> poolTypeTpPoolItemDictionary = new Dictionary<PoolObjectType, Pool>();

        
        protected override void Awake()
        {
            base.Awake();
            CreatePools();
        }

        private void CreatePools()
        {
            foreach (var poolObject in poolObjects.GetIPoolables())
            {
                var poolObjectType = poolObject.Key.PoolObjectType;
                var pool = new Pool(poolObject.Key);
                poolTypeTpPoolItemDictionary.Add(poolObjectType, pool);

                CreateInitialSpawn(poolObjectType, poolObject.Value);
            }
        }

        private void CreateInitialSpawn(PoolObjectType poolObjectType, int numberToSpawn)
        {
            if (numberToSpawn <= 0)
                return;

            Stack<IPoolable> pooledObjectsTemp = new();
            for (int i = 0;i < numberToSpawn;i++)
                pooledObjectsTemp.Push(Get(poolObjectType));

            for (int i = 0;i < numberToSpawn;i++)
                Release(pooledObjectsTemp.Pop());
        }

        public IPoolable Get(PoolObjectType poolObjectType, Transform parent = null)
        {
            if(poolObjectType == PoolObjectType.NULL)
            {
                Debug.LogError("PoolManager: Get, tried to Get NULL type!");
                return null;
            }

            IPoolable poolable = poolTypeTpPoolItemDictionary[poolObjectType].Pop();

            poolable.Initialize(parent != null ? parent : ingameParent);

            return poolable;
        }

        public void Release(IPoolable poolObject)
        {
            var pool = poolTypeTpPoolItemDictionary[poolObject.PoolObjectType];
            poolObject.ResetObject(pooledObjectParent);
            pool.Push(poolObject);
        }

        public void ResetPools()
        {
            foreach (var poolObjectType in poolTypeTpPoolItemDictionary)
            {
                ResetPool(poolObjectType.Key);
            }
        }

        public void ResetPool(PoolObjectType poolObjectType)
        {
            poolTypeTpPoolItemDictionary[poolObjectType].Reset();
        }
    }

    public class Pool
    {
        private readonly Stack<IPoolable> pooledObjects = new Stack<IPoolable>();
        private readonly List<IPoolable> activeObjects = new List<IPoolable>();
        private readonly IPoolable prefab;

        private int PooledObjectCount => pooledObjects.Count;
        private int ActiveObjectCount => activeObjects.Count;

        public Pool(IPoolable prefab)
        {
            this.prefab = prefab;
        }

        public IPoolable Pop()
        {
            IPoolable poolObject = PooledObjectCount > 0 ? pooledObjects.Pop() : (IPoolable)GameObject.Instantiate((UnityEngine.Object)prefab);
            activeObjects.Add(poolObject);
            return poolObject;
        }

        public void Push(IPoolable poolObject)
        {
            pooledObjects.Push(poolObject);
            activeObjects.Remove(poolObject);
        }


        public void Reset()
        {
            int count = ActiveObjectCount;
            for(int i = count - 1;i >= 0;--i)
            {
                activeObjects[i].ResetObject();
                pooledObjects.Push(activeObjects[i]);
                activeObjects.Remove(activeObjects[i]);
            }
            activeObjects.Clear();
        }
    }

    public enum PoolObjectType
    {
        Visitor,
        Guide,
        NULL
    }
}
