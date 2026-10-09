using System;
using System.Collections.Generic;
using System.IO;
using Shop.Model;
using UnityEngine;

namespace Shop.Core
{
    /// <summary>Where the save JSON lives. Abstracted so the logic can be unit-tested without touching disk.</summary>
    public interface ISaveStorage
    {
        /// <summary>Candidate save contents, best (most recent) first.</summary>
        IEnumerable<string> ReadCandidates();
        void Write(string json);
    }

    /// <summary>
    /// Stores the save in a plain file under persistentDataPath, which survives killing the app.
    /// The previous version is kept as ".bak" so a write interrupted by a kill cannot lose the save.
    /// </summary>
    public sealed class FileSaveStorage : ISaveStorage
    {
        private readonly string _path;
        private readonly string _tempPath;
        private readonly string _backupPath;

        public FileSaveStorage(string directory, string fileName = "shop_save.json")
        {
            _path = Path.Combine(directory, fileName);
            _tempPath = _path + ".tmp";
            _backupPath = _path + ".bak";
        }

        public IEnumerable<string> ReadCandidates()
        {
            foreach (var candidate in new[] { _path, _backupPath })
            {
                string text = null;
                try
                {
                    if (File.Exists(candidate))
                        text = File.ReadAllText(candidate);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Shop] Could not read '{candidate}': {e.Message}");
                }

                if (!string.IsNullOrEmpty(text))
                    yield return text;
            }
        }

        public void Write(string json)
        {
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(_tempPath, json);
            if (File.Exists(_path))
                File.Copy(_path, _backupPath, true);
            File.Copy(_tempPath, _path, true);
            File.Delete(_tempPath);
        }
    }

    /// <summary>Loads/saves <see cref="SaveData"/>, falling back to older copies and finally to a fresh save.</summary>
    public sealed class SaveRepository
    {
        private readonly ISaveStorage _storage;

        public SaveData Data { get; private set; }

        public SaveRepository(ISaveStorage storage)
        {
            _storage = storage;
        }

        public SaveData Load(int startingCoins)
        {
            foreach (var json in _storage.ReadCandidates())
            {
                try
                {
                    var data = JsonUtility.FromJson<SaveData>(json);
                    if (data == null)
                        continue;

                    Normalize(data);
                    Data = data;
                    return Data;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Shop] Ignoring unreadable save: {e.Message}");
                }
            }

            Data = new SaveData { coins = Mathf.Max(0, startingCoins) };
            Save();
            return Data;
        }

        public void Save()
        {
            if (Data == null)
                return;

            try
            {
                _storage.Write(JsonUtility.ToJson(Data));
            }
            catch (Exception e)
            {
                Debug.LogError($"[Shop] Saving failed: {e.Message}");
            }
        }

        private static void Normalize(SaveData data)
        {
            data.ownedItems = data.ownedItems ?? new List<string>();
            data.powerUps = data.powerUps ?? new List<CountEntry>();
            data.equipped = data.equipped ?? new List<EquipEntry>();
            data.coins = Mathf.Max(0, data.coins);
        }
    }
}
