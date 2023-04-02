using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace FakePeppino
{
    internal class FakePeppino : Mod, ILocalSettings<LocalSettings>
    {
        internal static FakePeppino Instance { get; private set; }

        public Dictionary<string, AudioClip> AudioClips { get; private set; } = new();
        public Dictionary<string, AssetBundle> Bundles { get; private set; } = new();
        public Dictionary<string, GameObject> GameObjects { get; private set; } = new();

        private Material _blurMat;

        private Dictionary<string, (string, string)> _preloads = new()
        {
            ["Boss Scene Controller"] = ("GG_Hornet_1", "Boss Scene Controller"),
            ["Godseeker"] = ("GG_Hornet_1", "Boss Holder/Godseeker Crowd"),
            ["Reference"] = ("GG_Hornet_1", "Boss Holder/Hornet Boss 1"),
        };

        private static LocalSettings _localSettings = new();

        public void OnLoadLocal(LocalSettings settings) => _localSettings = settings;

        public LocalSettings OnSaveLocal() => _localSettings;

        public FakePeppino() : base("Fake Peppino") { }

        public override List<(string, string)> GetPreloadNames() => _preloads.Values.ToList();

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;

            foreach (var (name, (scene, path)) in _preloads)
            {
                GameObjects[name] = preloadedObjects[scene][path];
            }

            Unload();
            _blurMat = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(mat => mat.shader.name.Contains("UI/Blur/UIBlur"));
            LoadAssets();

            ModHooks.AfterSavegameLoadHook += AfterSaveGameLoad;
            ModHooks.GetPlayerVariableHook += GetVariableHook;
            ModHooks.LanguageGetHook += LangGet;
            ModHooks.NewGameHook += AddComponent;
            ModHooks.SetPlayerVariableHook += SetVariableHook;

            On.BlurPlane.Awake += OnBlurPlaneAwake;
            On.SceneManager.Start += OnSceneManagerStart;
            On.tk2dTileMap.Awake += OnTileMapAwake;
        }

        private void AfterSaveGameLoad(SaveGameData data) => AddComponent();

        private object GetVariableHook(Type t, string key, object orig)
        {
            if (key == "statueStatePep")
            {
                return _localSettings.Completion;
            }

            return orig;
        }

        private string LangGet(string key, string sheetTitle, string orig)
        {
            switch (key)
            {
                case "PEP_MAIN": return "Fake Peppino";
                case "PEP_SUB": return "";
                case "PEP_SUPER": return "";
                case "PEP_NAME": return "Fake Peppino";
                case "PEP_DESC": return "False god of the tower.";
                case "PEP_1": return "PEPPINO PEPPINO PEPPINO PEPPINO PEPPINO PEPPINO PEPPINO PEPPINO PEPPINO";
                case "PEP_2": return "FRTXJPIHCBATTZRDIHJPS!";
                case "PEP_3": return "PIZZA TIME NEVER ENDS";
                case "PEP_4": return "PIZZA PIZZA PIZZA PIZZA PIZZA PIZZA PIZZA PIZZA PIZZA PIZZA PIZZA PIZZA";
            }

            return orig;
        }

        private void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<StatueCreator>();
            GameManager.instance.gameObject.AddComponent<SceneLoader>();
        }

        private object SetVariableHook(Type t, string key, object obj)
        {
            if (key == "statueStatePep")
            {
                _localSettings.Completion= (BossStatue.Completion)obj;
            }

            return obj;
        }


        private void OnBlurPlaneAwake(On.BlurPlane.orig_Awake orig, BlurPlane self)
        {
            orig(self);

            if (self.OriginalMaterial.shader.name == "UI/Default")
            {
                self.SetPlaneMaterial(_blurMat);
            }
        }

        private void OnSceneManagerStart(On.SceneManager.orig_Start orig, SceneManager self)
        {
            orig(self);

            self.tag = "SceneManager";
        }

        private void OnTileMapAwake(On.tk2dTileMap.orig_Awake orig, tk2dTileMap self)
        {
            orig(self);

            self.tag = "TileMap";
        }

        private void LoadAssets()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) continue;

                    if (resourceName.Contains("fakepeppino"))
                    {
                        var bundle = AssetBundle.LoadFromStream(stream);
                        Bundles.Add(bundle.name, bundle);
                    }
                    else if (resourceName.Contains("GG_Statue_AspidQueen"))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        var statueTex = new Texture2D(2, 2);
                        statueTex.LoadImage(buffer); 
                        // Textures.Add("GG_Statue_AspidQueen", statueTex);
                    }

                    stream.Dispose();
                }
            }
        }

        internal IEnumerator DreamReturnDelayed()
        {
            StatueCreator.WonFight = true;

            yield return new WaitForSeconds(6);

            var bsc = SceneLoader.SceneController.GetComponent<BossSceneController>();
            GameObject transition = UObject.Instantiate(bsc.transitionPrefab);
            PlayMakerFSM transitionsFSM = transition.LocateMyFSM("Transitions");
            transitionsFSM.SetState("Out Statue");
            yield return new WaitForSeconds(1);
            bsc.DoDreamReturn();
        }

        private void Unload()
        {
            ModHooks.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.GetPlayerVariableHook -= GetVariableHook;
            ModHooks.LanguageGetHook -= LangGet;
            ModHooks.SetPlayerVariableHook -= SetVariableHook;
            ModHooks.NewGameHook -= AddComponent;

            On.BlurPlane.Awake -= OnBlurPlaneAwake;
            On.SceneManager.Start -= OnSceneManagerStart;
            On.tk2dTileMap.Awake -= OnTileMapAwake;

            var statueCreator = GameManager.instance?.gameObject.GetComponent<StatueCreator>();
            if (statueCreator == null)
            {
                return;
            }

            UObject.Destroy(statueCreator);
        }
    }
}