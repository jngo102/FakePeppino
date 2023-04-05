using HutongGames.PlayMaker.Actions;
using Modding;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Vasi;

namespace FakePeppino
{
    [RequireComponent(typeof(EnemyDeathEffectsUninfected))]
    [RequireComponent(typeof(EnemyDreamnailReaction))]
    [RequireComponent(typeof(HealthManager))]
    [RequireComponent(typeof(EnemyHitEffectsUninfected))]
    internal class PeppinoManager : MonoBehaviour
    {
        private GameObject _audioPlayer;
        private EnemyDeathEffectsUninfected _deathEffects;
        private EnemyDreamnailReaction _dreamReaction;
        private HealthManager _healthManager;
        private EnemyHitEffectsUninfected _hitEffects;
            
        private void Awake()
        {
            _deathEffects = GetComponent<EnemyDeathEffectsUninfected>();
            _dreamReaction = GetComponent<EnemyDreamnailReaction>();
            _healthManager = GetComponent<HealthManager>();
            _hitEffects = GetComponent<EnemyHitEffectsUninfected>();

            CopyFields();
            AssignMissing();

            On.PlayMakerFSM.Awake += OnPFSMAwake;
        }

        private void OnDestroy()
        {
            On.PlayMakerFSM.Awake -= OnPFSMAwake;
        }
        
        private void CopyFields()
        {
            var @ref = FakePeppino.Instance.GameObjects["Reference"];
            
            var deathEffects = @ref.GetComponent<EnemyDeathEffectsUninfected>();
            deathEffects.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).ToList()
                .ForEach(fi => fi.SetValue(_deathEffects, fi.GetValue(deathEffects)));

            var dreamReaction = @ref.GetComponent<EnemyDreamnailReaction>();
            ReflectionHelper.SetField(_dreamReaction, "dreamImpactPrefab",
                ReflectionHelper.GetField<EnemyDreamnailReaction, GameObject>(dreamReaction, "dreamImpactPrefab"));

            var healthManager = @ref.GetComponent<HealthManager>();
            healthManager.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(fi => fi.Name.Contains("Audio") || fi.Name.Contains("Prefab")).ToList()
                .ForEach(fi => fi.SetValue(_healthManager, fi.GetValue(healthManager)));

            var hitEffects = @ref.GetComponent<EnemyHitEffectsUninfected>();
            hitEffects.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).ToList()
                .ForEach(fi => fi.SetValue(_hitEffects, fi.GetValue(hitEffects)));
        }

        private void AssignMissing()
        {
            _audioPlayer = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go =>
                go.name == "Audio Player Actor" && go.GetComponent<PlayAudioAndRecycle>());
            
            PlayMakerFSM control = gameObject.LocateMyFSM("Control");
            control.GetState("Vulnerable").GetAction<SpawnObjectFromGlobalPool>().gameObject.Value =
                control.GetState("Stunned").GetAction<SpawnObjectFromGlobalPool>().gameObject.Value = Resources
                    .FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == "Stun Effect");

            foreach (var fsm in GetComponents<PlayMakerFSM>())
            {
                foreach (var state in fsm.FsmStates)
                {
                    foreach (var action in state.Actions)
                    {
                        if (action is AudioPlayerOneShotSingle single)
                        {
                            if (single.audioPlayer.Value == null)
                            {
                                single.audioPlayer.Value = _audioPlayer;
                            }
                        }
                    }
                }
            }
        }

        private void OnPFSMAwake(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM fsm)
        {
            if (fsm.name.Contains("Silhouette") && fsm.FsmName == "Placeholder")
            {
                fsm.gameObject.AddComponent<Silhouette>();
                Destroy(fsm);
            }
            else if (fsm.name.Contains("Dummy Peppino"))
            {
                foreach (var state in fsm.FsmStates)
                {
                    foreach (var action in state.Actions)
                    {
                        if (action is AudioPlayerOneShotSingle single)
                        {
                            if (single.audioPlayer.Value == null)
                            {
                                single.audioPlayer.Value = _audioPlayer;
                            }
                        }
                    }
                }
            }
            
            orig(fsm);
        }
    }
}
