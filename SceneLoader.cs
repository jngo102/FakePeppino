using System.Linq;
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
        private int _bossLevel;
        private int _savedBossLevel;

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
                var bscObj = Instantiate(FakePeppino.GameObjects["Boss Scene Controller"]);
                bscObj.SetActive(true);
                var bsc = bscObj.GetComponent<BossSceneController>();
                if (nextScene.name == "FakePeppino") 
                {
                    _savedBossLevel = bsc.BossLevel;
                } 
                else if (nextScene.name == "Chase")
                {
                    bsc.BossLevel = _bossLevel = _savedBossLevel;

                    Destroy(bscObj.Child("Dream Entry"));

                    var audioSource = GameObject.Find("Peppino Chaser").GetComponent<AudioSource>();
                    audioSource.outputAudioMixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().FirstOrDefault(group =>
                        group.name == "Actors" && group.audioMixer.outputAudioMixerGroup.name == "Actors");
                }
                else if (nextScene.name == "Victory")
                {
                    Destroy(bscObj.Child("Dream Entry"));

                    var audioSource = GameObject.Find("Audio Player Actor").GetComponent<AudioSource>();
                    audioSource.outputAudioMixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().FirstOrDefault(group =>
                        group.name == "Actors" && group.audioMixer.outputAudioMixerGroup.name == "Actors");

                    var pd = PlayerData.instance;
                    var completion = pd.GetVariable<BossStatue.Completion>(FakePeppino.Instance.PlayerData);
                    switch (_bossLevel)
                    {
                        case 0:
                            completion.completedTier1 = true;
                            break;
                        case 1:
                            completion.completedTier2 = true;
                            break;
                        case 2:
                            completion.completedTier3 = true;
                            break;
                    }
                    if (completion.completedTier1 && completion.completedTier2 && !completion.completedTier3)
                    {
                        completion.seenTier3Unlock = true;
                    }

                    pd.SetVariable(FakePeppino.Instance.PlayerData, completion);

                    StartCoroutine(FakePeppino.Instance.DreamReturnDelayed(9));

                    _savedBossLevel = 0;
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
            else if (nextScene.name == "GG_Workshop")
            {
                if (prevScene.name == "Victory")
                {
                    var pd = PlayerData.instance;
                    var completion = pd.GetVariable<BossStatue.Completion>(FakePeppino.Instance.PlayerData);
                    var bs = FindObjectsOfType<BossStatue>().FirstOrDefault(statue => statue.bossDetails.nameKey == "PEP_NAME");
                    PlayerData.instance.currentBossStatueCompletionKey = bs.statueStatePD;
                    bs.StatueState = completion;
                    bs.SetPlaqueState(bs.StatueState, bs.regularPlaque, bs.statueStatePD);
                }
            }
        }

        private void OnDestroy()
        {
            On.GameManager.EnterHero -= OnEnterHero;
        }
    }
}
