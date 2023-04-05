using HutongGames.PlayMaker.Actions;
using Modding;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using Vasi;

namespace FakePeppino
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(EnemyDeathEffectsUninfected))]
    [RequireComponent(typeof(EnemyDreamnailReaction))]
    [RequireComponent(typeof(HealthManager))]
    [RequireComponent(typeof(EnemyHitEffectsUninfected))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteFlash))]
    [RequireComponent(typeof(tk2dSpriteAnimator))]
    internal class PeppinoManager : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Collider2D _collider;
        private EnemyDeathEffectsUninfected _deathEffects;
        private EnemyDreamnailReaction _dreamReaction;
        private HealthManager _healthManager;
        private EnemyHitEffectsUninfected _hitEffects;
        private ParticleSystem _particles;
        private PlayMakerFSM _control;
        private PlayMakerFSM _dummySpawner;
        private Rigidbody2D _rigidbody;
        private SpriteFlash _spriteFlash;
        private tk2dSpriteAnimator _animator;

        private void Awake()
        {
            _control = gameObject.LocateMyFSM("Control");
            _dummySpawner = gameObject.LocateMyFSM("Dummy Spawner");

            _audioSource = GetComponent<AudioSource>();
            _collider = GetComponent<Collider2D>();
            _deathEffects = GetComponent<EnemyDeathEffectsUninfected>();
            _dreamReaction = GetComponent<EnemyDreamnailReaction>();
            _healthManager = GetComponent<HealthManager>();
            _hitEffects = GetComponent<EnemyHitEffectsUninfected>();
            _particles = GetComponentInChildren<ParticleSystem>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteFlash = GetComponent<SpriteFlash>();
            _animator = GetComponent<tk2dSpriteAnimator>();
            
            _healthManager.OnDeath += OnDeath;

            CopyFields();
        }

        private void Start()
        {
            var audioPlayer = GameManager.instance.transform.Find("GlobalPool/Audio Player Actor(Clone)").gameObject;
            //var audioPlayer = new GameObject("Audio Player Actor");
            //audioPlayer.SetActive(false);
            //var audioSource = audioPlayer.AddComponent<AudioSource>();
            var mixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>()
                .FirstOrDefault(group => group.name == "Actors" && group.audioMixer.name == "Actors");
            //audioPlayer.AddComponent<PlayAudioAndRecycle>().audioSource = audioSource;

            foreach (var fsm in GetComponents<PlayMakerFSM>())
            {
                foreach (var state in fsm.FsmStates)
                {
                    foreach (var action in state.Actions)
                    {
                        if (action is AudioPlayerOneShotSingle single)
                        {
                            single.audioPlayer.Value = audioPlayer;
                        }
                    }
                }
            }

            _control.GetState("Vulnerable").GetAction<SpawnObjectFromGlobalPool>().gameObject.Value =
                _control.GetState("Stunned").GetAction<SpawnObjectFromGlobalPool>().gameObject.Value =
                    GameManager.instance.transform.Find("GlobalPool/Stun Effect(Clone)").gameObject;
            
            var dummy = _dummySpawner.GetState("Spawn").GetAction<CreateObject>().gameObject.Value;
            var dummyMaterial = dummy.GetComponent<MeshRenderer>().material;
            var materialClone = Instantiate(dummyMaterial);
            materialClone.SetColor("_ReplacementColor", new Color(0.75f, 0.75f, 0.75f));
            _dummySpawner.GetState("Spawn").GetAction<CreateObject>().gameObject.Value.GetComponent<MeshRenderer>()
                .material = materialClone;
            /*audioSource.outputAudioMixerGroup = */_audioSource.outputAudioMixerGroup =
                dummy.GetComponent<AudioSource>().outputAudioMixerGroup = mixerGroup;
            foreach (var fsm in dummy.GetComponents<PlayMakerFSM>())
            {
                foreach (var state in fsm.FsmStates)
                {
                    foreach (var action in state.Actions)
                    {
                        if (action is AudioPlayerOneShotSingle single)
                        {
                            single.audioPlayer.Value = audioPlayer;
                        }
                    }
                }
            }
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
        
        private void OnDeath()
        {
            
        }
    }
}
