using System.IO;
using Shop.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shop.Runtime
{
    /// <summary>
    /// Composition root of the shop feature. It bootstraps itself before the first scene loads (no scene
    /// or prefab has to be edited for it to exist), survives scene changes, loads the JSON config and the
    /// save file, and plugs the feature into the game scene when it loads.
    /// </summary>
    public sealed class ShopSystem : MonoBehaviour
    {
        private const string GameSceneName = "Scene_Game";
        private const string ConfigResource = "Shop/ShopConfig";
        // Optional: drop a file with this name into persistentDataPath to try other prices/items on a device.
        private const string ConfigOverrideFile = "shop_config.override.json";

        public static ShopSystem Instance { get; private set; }

        public ShopCatalog Catalog { get; private set; }
        public ShopService Service { get; private set; }
        public ShopResources Art { get; private set; }

        private SaveRepository _repository;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
                return;

            var host = new GameObject("[ShopSystem]");
            DontDestroyOnLoad(host);
            host.AddComponent<ShopSystem>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Art = ShopResources.Load();
            Catalog = ShopCatalog.FromJson(ReadConfigJson());
            _repository = new SaveRepository(new FileSaveStorage(Application.persistentDataPath));
            var data = _repository.Load(Catalog.Economy.startingCoins);
            Service = new ShopService(Catalog, data, _repository.Save);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                _repository?.Save();
        }

        private void OnApplicationQuit()
        {
            _repository?.Save();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != GameSceneName)
                return;

            // Runs before the scene's Start() calls, so the ball captures its themed colour as its "original" one.
            ThemeApplier.Apply(scene, Service);
            GameSession.Create(scene, this);
        }

        private static string ReadConfigJson()
        {
            try
            {
                var overridePath = Path.Combine(Application.persistentDataPath, ConfigOverrideFile);
                if (File.Exists(overridePath))
                    return File.ReadAllText(overridePath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Shop] Ignoring config override: {e.Message}");
            }

            var asset = Resources.Load<TextAsset>(ConfigResource);
            if (asset == null)
            {
                Debug.LogError($"[Shop] Missing Resources/{ConfigResource}.json");
                return "{}";
            }

            return asset.text;
        }
    }
}
