﻿using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Vasi;
using UObject = UnityEngine.Object;
using BossStatueFramework;

namespace FakePeppino
{
    internal class FakePeppino : Mod, IBossStatueMod
    {
        internal static FakePeppino Instance { get; private set; }
        
        public Dictionary<string, AssetBundle> Bundles { get; } = new();
        public static Dictionary<string, GameObject> GameObjects { get; } = new();
        public static Texture2D StatueTex;

        public string NameKey => "PEP_NAME";
        public string DescriptionKey => "PEP_DESC";

        public string SceneName => "FakePeppino";

        public PlinthType PlinthType => PlinthType.Medium;

        public Sprite Sprite => Sprite.Create(StatueTex, new Rect(0, 0, StatueTex.width, StatueTex.height), new Vector2(0.5f, 0.5f));

        public float SpriteScale => 2;

        public string PlayerData => "statueStatePep";

        public AltSwitchType AltType => AltSwitchType.None;

        public string AltNameKey => "";

        public string AltDescriptionKey => "";

        public string AltSceneName => "GG_Hollow_Knight";

        public string AltPlayerData => "";

        public Sprite AltSprite => null;

        public float AltSpriteScale => 0;

        private Dictionary<string, (string, string)> _preloads = new()
        {
            ["Boss Scene Controller"] = ("GG_Hornet_1", "Boss Scene Controller"),
            ["Reference"] = ("GG_Hornet_1", "Boss Holder/Hornet Boss 1"),
        };

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
            LoadAssets();

            ModHooks.AfterSavegameLoadHook += AfterSaveGameLoad;
            ModHooks.LanguageGetHook += LangGet;
            ModHooks.NewGameHook += AddComponent;
        }

        private void AfterSaveGameLoad(SaveGameData data) => AddComponent();


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
            GameManager.instance.gameObject.AddComponent<SceneLoader>();
        }

        private void LoadAssets()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) continue;

                    if (resourceName.Contains("GG_Statue_FakePeppino"))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        var statueTex = new Texture2D(2, 2);
                        statueTex.LoadImage(buffer);
                        StatueTex = statueTex;
                        continue;
                    }

                    var bundle = AssetBundle.LoadFromStream(stream);
                    Bundles.Add(bundle.name, bundle);

                    stream.Dispose();
                }
            }
        }

        internal IEnumerator DreamReturnDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);

            var bsc = BossSceneController.Instance;
            var bscObj = bsc.gameObject;
            bscObj.SetActive(true);
            var dreamReturn = bscObj.LocateMyFSM("Dream Return");
            dreamReturn.GetState("Statue").GetAction<GetPlayerDataString>().storeValue = "GG_Workshop";
            dreamReturn.Fsm.GetFsmString("Entry Gate").Value =
                $"door_dreamReturnGG_GG_Statue_{nameof(FakePeppino)}";
            dreamReturn.Fsm.GetFsmString("Return Scene").Value = "GG_Workshop";
            bsc.DreamReturnEvent = "DREAM RETURN";
            bsc.bossesDeadWaitTime = delay;
            GameObject transition = UObject.Instantiate(bsc.transitionPrefab);
            PlayMakerFSM transitionsFSM = transition.LocateMyFSM("Transitions");
            transitionsFSM.SetState("Out Statue");
            yield return new WaitForSeconds(1);
            bsc.DoDreamReturn();
        }

        private void Unload()
        {
            ModHooks.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.LanguageGetHook -= LangGet;
            ModHooks.NewGameHook -= AddComponent;
        }
    }
}