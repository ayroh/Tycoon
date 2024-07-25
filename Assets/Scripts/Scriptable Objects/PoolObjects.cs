using Pool;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/PoolObjects")]
public class PoolObjects : ScriptableObject
{
    [SerializeField] private Visitor visitor;

    public Dictionary<IPoolable, int> GetIPoolables()
    {
        return new Dictionary<IPoolable, int>
        {
            { visitor, 15 }
        };
    }
}