using System.Linq;
using HutongGames.PlayMaker.Actions;
using Modding.Utils;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityStandardAssets.ImageEffects;
using Vasi;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace FakePeppino
{
    internal class SceneLoader : MonoBehaviour
    {
        internal static BossSceneController SceneController;

        private void Awake()
        {
            On.GameManager.EnterHero += OnEnterHero;
            USceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnEnterHero(On.GameManager.orig_EnterHero orig, GameManager gm, bool additiveGateSearch)
        {
            if (gm.sceneName == "FakePeppino")
            {
                GameObject.Find("Fake Peppino").AddComponent<PeppinoManager>();
            }

            orig(gm, additiveGateSearch);

            var keepHealth = HeroController.instance.gameObject.GetOrAddComponent<KeepHealthOnSceneChange>();

            if (gm.sceneName == "FakePeppino")
            {
                HeroController.instance.transform.SetPosition2D(14, 4);
            }

            if (gm.sceneName == "Chase" || gm.sceneName == "Victory")
            {
                keepHealth?.ResetHealth();
                if (gm.sceneName == "Victory" && keepHealth != null)
                {
                    Destroy(keepHealth);
                }
            }

            if (gm.sceneName == "FakePeppino" || gm.sceneName == "Chase" || gm.sceneName == "Victory")
            {
                HeroController.instance.transform.Find("Vignette").gameObject.SetActive(false);
                GameCameras.instance.tk2dCam.GetComponent<BloomOptimized>().enabled = false;
            }
            else
            {
                if (keepHealth != null)
                {
                    Destroy(keepHealth);
                }
                HeroController.instance.transform.Find("Vignette").gameObject.SetActive(true);
                GameCameras.instance.tk2dCam.GetComponent<BloomOptimized>().enabled = true;
            }   
        }

        private void OnSceneChange(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "FakePeppino" || nextScene.name == "Chase" || nextScene.name == "Victory")
            {
                var bsc = Instantiate(FakePeppino.GameObjects["Boss Scene Controller"]);
                bsc.SetActive(true);
                SceneController = bsc.GetComponent<BossSceneController>();
                if (nextScene.name == "FakePeppino")
                {
                    StatueCreator.BossLevel = SceneController.BossLevel;
                }
                else if (nextScene.name == "Chase")
                {
                    SceneController.BossLevel = StatueCreator.BossLevel;
                    Destroy(bsc.Child("Dream Entry"));

                    var audioSource = GameObject.Find("Peppino Chaser").GetComponent<AudioSource>();
                    audioSource.outputAudioMixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().FirstOrDefault(group =>
                        group.name == "Actors" && group.audioMixer.outputAudioMixerGroup.name == "Actors");
                }
                else if (nextScene.name == "Victory")
                {
                    SceneController.BossLevel = StatueCreator.BossLevel;
                    Destroy(bsc.Child("Dream Entry"));

                    var audioSource = GameObject.Find("Audio Player Actor").GetComponent<AudioSource>();
                    audioSource.outputAudioMixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().FirstOrDefault(group =>
                        group.name == "Actors" && group.audioMixer.outputAudioMixerGroup.name == "Actors");

                    StartCoroutine(FakePeppino.Instance.DreamReturnDelayed(9));
                }

                var rootGOs = nextScene.GetRootGameObjects();
                foreach (var go in rootGOs)
                {
                    foreach (var sprRend in go.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        sprRend.material.shader = Shader.Find("Sprites/Default");
                    }

                    foreach (var sprRend in go.GetComponentsInChildren<TilemapRenderer>(true))
                    {
                        sprRend.material.shader = Shader.Find("Sprites/Default");
                    }
                }
            }
        }

        private void OnDestroy()
        {
            On.GameManager.EnterHero -= OnEnterHero;
        }
    }
}
