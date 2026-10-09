using UnityEngine;
using System;
using NaughtyAttributes;


namespace ScriptableObjects.GameEvents
{
[CreateAssetMenu(menuName = "GameEvents/NullEvent")]
    public class NullEvent : ScriptableObject
    {
        public Action OnEventRaised;
        public void Raise()
        {
            OnEventRaised?.Invoke();
        }
        [Button]
        public void RaiseTest()
        {
            OnEventRaised?.Invoke();
        }
    }
}
