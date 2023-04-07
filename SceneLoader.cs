using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityStandardAssets.ImageEffects;
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

            if (gm.sceneName == "FakePeppino")
            {
                HeroController.instance.transform.SetPosition2D(14, 4);
            }

            if (gm.sceneName == "FakePeppino" || gm.sceneName == "Chase" || gm.sceneName == "Victory")
            {
                HeroController.instance.transform.Find("Vignette").gameObject.SetActive(false);
                GameCameras.instance.tk2dCam.GetComponent<BloomOptimized>().enabled = false;
            }
            else
            {
                HeroController.instance.transform.Find("Vignette").gameObject.SetActive(true);
                GameCameras.instance.tk2dCam.GetComponent<BloomOptimized>().enabled = true;
            }   
        }

        private void OnSceneChange(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "FakePeppino")
            {
                if (SceneController != null)
                {
                    Destroy(SceneController.gameObject);
                }
                
                var bsc = Instantiate(FakePeppino.Instance.GameObjects["Boss Scene Controller"]);
                bsc.SetActive(true);
                SceneController = bsc.GetComponent<BossSceneController>();
                StatueCreator.BossLevel = SceneController.BossLevel;
            }

            if (nextScene.name == "FakePeppino" || nextScene.name == "Chase" || nextScene.name == "Victory")
            {
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

            if (nextScene.name == "Victory")
            {
                StartCoroutine(FakePeppino.Instance.DreamReturnDelayed(9));
            }
        }

        private void OnDestroy()
        {
            On.GameManager.EnterHero -= OnEnterHero;
        }
    }
}
