using System.IO;
using Shop.Core;
using Shop.Runtime;
using UnityEditor;
using UnityEngine;

namespace Shop.EditorTools
{
    /// <summary>Editor shortcuts to test the shop without grinding coins: Tools > Shop.</summary>
    public static class ShopDebugMenu
    {
        private const string SaveFileName = "shop_save.json";

        [MenuItem("Tools/Shop/Add 1000 Coins")]
        private static void AddCoins()
        {
            if (Application.isPlaying && ShopSystem.Instance != null)
            {
                ShopSystem.Instance.Service.AddCoins(1000);
                return;
            }

            // Not playing: edit the save file directly (takes effect on the next Play).
            var repository = new SaveRepository(new FileSaveStorage(Application.persistentDataPath));
            var data = repository.Load(0);
            data.coins += 1000;
            repository.Save();
            Debug.Log($"[Shop] Save now has {data.coins} coins.");
        }

        [MenuItem("Tools/Shop/Reset Save")]
        private static void ResetSave()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[Shop] Stop Play mode before resetting the save.");
                return;
            }

            foreach (var suffix in new[] { string.Empty, ".bak", ".tmp" })
            {
                var path = Path.Combine(Application.persistentDataPath, SaveFileName + suffix);
                if (File.Exists(path))
                    File.Delete(path);
            }

            Debug.Log("[Shop] Save deleted.");
        }

        [MenuItem("Tools/Shop/Open Save Folder")]
        private static void OpenSaveFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
}
