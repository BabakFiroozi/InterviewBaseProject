using Export;
using UnityEngine;

namespace ScriptableObjects.Services
{
    [CreateAssetMenu(menuName = "Services/Player Prefs Save Service", fileName = "PlayerPrefsSaveService")]
    public class PlayerPrefsSaveService : ScriptableObject
    {
        public static PlayerPrefsSaveService Main
        {
            get
            {
                if (!_main)
                {
                    _main = Resources.Load<PlayerPrefsSaveService>("PlayerPrefsSaveService");
                }
                return _main;
            }
        }
        private static PlayerPrefsSaveService _main;

        
        public void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            PlayerDataJsonExporter.SaveJsonFromPlayerPrefs();
        }

        public int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            PlayerDataJsonExporter.SaveJsonFromPlayerPrefs();
        }

        public float LoadFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
            PlayerDataJsonExporter.SaveJsonFromPlayerPrefs();
        }

        public string LoadString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
    }
}
