using UnityEngine;
using UnityEngine.SceneManagement;
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
                var bsc = Instantiate(FakePeppino.Instance.GameObjects["Boss Scene Controller"]);
                bsc.SetActive(true);
                SceneController = bsc.GetComponent<BossSceneController>();
                StatueCreator.BossLevel = SceneController.BossLevel;

                var rootGOs = nextScene.GetRootGameObjects();
                foreach (var go in rootGOs)
                {
                    foreach (var sprRend in go.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        sprRend.material.shader = Shader.Find("Sprites/Default");
                    }

                    foreach (var meshRend in go.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        meshRend.material.shader = Shader.Find(meshRend.GetComponent<BlurPlane>() ? "UI/Blur/UIBlur" : "Sprites/Default-ColorFlash");
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
