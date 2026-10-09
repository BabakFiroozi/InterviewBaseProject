using Enums;

namespace ScriptableObjects.UIConfigs
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "UI/PanelSequence")]
    public class PanelSequence : ScriptableObject
    {
        public TabType[] sequence;

        public static PanelSequence Main
        {
            get
            {
                if (_main == null)
                {
                    _main = Resources.Load<PanelSequence>("ScriptableObject_Panel_Sequence");
                }
                return _main;
            }
        }
        private static PanelSequence _main;

        public TabType firstActiveTab;
        
    }

}