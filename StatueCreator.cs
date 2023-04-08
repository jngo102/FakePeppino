using UnityEngine;
using UnityEngine.SceneManagement;

namespace FakePeppino
{
    internal class StatueCreator : MonoBehaviour
    {
        internal static bool WonFight;
        internal static int BossLevel;

        private void Awake()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        }

        private void SceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "GG_Workshop")
            {
                CreateStatue();
            }
        }

        private void CreateStatue()
        {
            GameObject statue = Instantiate(GameObject.Find("GG_Statue_Mage_Knight"));
#if DEBUG 
            statue.transform.position += Vector3.left * 22;
#else
            statue.transform.position += Vector3.left * 8;
#endif

            var scene = ScriptableObject.CreateInstance<BossScene>();
            scene.sceneName = "FakePeppino";

            var bs = statue.GetComponent<BossStatue>();
            bs.name = "GG_Statue_Fake_Peppino";
            bs.bossScene = scene;
            bs.statueStatePD = "statueStatePep";
            bs.SetPlaquesVisible(bs.StatueState.isUnlocked && bs.StatueState.hasBeenSeen);

            var details = new BossStatue.BossUIDetails();
            details.nameKey = details.nameSheet = "PEP_NAME";
            details.descriptionKey = details.descriptionSheet = "PEP_DESC";
            bs.bossDetails = details;

            GameObject appearance = statue.transform.Find("Base").Find("Statue").gameObject;
            appearance.SetActive(true);
             var statueTex = FakePeppino.StatueTex;
            SpriteRenderer sr = appearance.transform.Find("GG_statues_0006_5").GetComponent<SpriteRenderer>();
            sr.enabled = true;
            sr.sprite = Sprite.Create(statueTex, new Rect(0, 0, statueTex.width, statueTex.height), new Vector2(0.5f, 0.5f));
            sr.transform.position += Vector3.left * 0.5f;
            sr.transform.localScale *= 2;

            GameObject inspect = statue.transform.Find("Inspect").gameObject;
            var tmp = inspect.transform.Find("Prompt Marker").position;
            inspect.transform.Find("Prompt Marker").position = new Vector3(tmp.x - 0.3f, tmp.y + 1.0f, tmp.z);
            inspect.SetActive(true);

            statue.transform.Find("Spotlight").gameObject.SetActive(true);

            if (WonFight)
            {
                WonFight = false;
                BossStatue.Completion temp = bs.StatueState;
                if (BossLevel == 0) temp.completedTier1 = true;
                else if (BossLevel == 1) temp.completedTier2 = true;
                else if (BossLevel == 2) temp.completedTier3 = true;
                if (temp.completedTier1 && temp.completedTier2 && !temp.seenTier3Unlock) temp.seenTier3Unlock = true;
                PlayerData.instance.currentBossStatueCompletionKey = bs.statueStatePD;
                bs.StatueState = temp;
                bs.SetPlaqueState(bs.StatueState, bs.altPlaqueL, bs.statueStatePD);
            }

            statue.SetActive(true);
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChanged;
        }
    }
}
