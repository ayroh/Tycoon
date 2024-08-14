
using System;
using UnityEngine;

namespace Pool
{
    public interface IPoolable
    {
        public abstract PoolObjectType PoolObjectType { get; }
        void Initialize(Transform parent = null);
        void ResetObject(Transform parent = null);

    }
}