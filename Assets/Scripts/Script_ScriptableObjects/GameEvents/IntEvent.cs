using UnityEngine;
using System;
using NaughtyAttributes;

namespace ScriptableObjects.GameEvents
{
[CreateAssetMenu(menuName = "GameEvents/IntEvent")]
    public class IntEvent : ScriptableObject
    {
        public Action<int> OnEventRaised;
        public void Raise(int value)
        {
            OnEventRaised?.Invoke(value);
        }


        [Header("For Test Event")]
        public int test = 0;
        [Button]
        public void RaiseTest()
        {
            OnEventRaised?.Invoke(test);
        }
    }
}
