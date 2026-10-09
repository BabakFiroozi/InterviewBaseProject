using System;
using System.IO;
using DataStructure;
using ScriptableObjects.Services;
using UnityEngine;

namespace Export
{
    public static class PlayerDataJsonExporter
    {
        private static string filePath => Path.Combine(Application.persistentDataPath, "info.json");
        

        public static void SaveJsonFromPlayerPrefs()
        {
            try
            {
                PlayerInfo info = new PlayerInfo
                {
                    rank = PlayerPrefsSaveService.Main.LoadInt("PlayerRank", 4510),
                    name = PlayerPrefsSaveService.Main.LoadString("PlayerName", "Honey Drops"),
                    score = PlayerPrefsSaveService.Main.LoadInt("Score", 6000),
                    isUser = true
                };

                string json = JsonUtility.ToJson(info, true);
                
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                    if (directory != null)
                        Directory.CreateDirectory(directory);

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save JSON: {ex}");
            }
        }
    }
}

    
