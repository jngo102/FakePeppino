using HutongGames.PlayMaker.Actions;
using Modding;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace FakePeppino
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    internal class PeppinoManager : MonoBehaviour
    {
        private Animator _animator;
        private Collider2D _collider;
        private EnemyDeathEffectsUninfected _deathEffects;
        private EnemyDreamnailReaction _dreamReaction;
        private HealthManager _healthManager;
        private EnemyHitEffectsUninfected _hitEffects;
        private ParticleSystem _particles;
        private PlayMakerFSM _control;
        private Rigidbody2D _rigidbody;
        private SpriteFlash _spriteFlash;

        private void Awake()
        {
            _control = gameObject.LocateMyFSM("Control");

            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider2D>();
            _deathEffects = GetComponent<EnemyDeathEffectsUninfected>();
            _dreamReaction = GetComponent<EnemyDreamnailReaction>();
            _healthManager = GetComponent<HealthManager>();
            _hitEffects = GetComponent<EnemyHitEffectsUninfected>();
            _particles = GetComponentInChildren<ParticleSystem>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteFlash = GetComponent<SpriteFlash>();

            _healthManager.hp = 500;
            _healthManager.hasSpecialDeath = true;
            _healthManager.OnDeath += OnDeath;

            CopyFields();
        }

        private IEnumerator Start()
        {
            //var audioPlayer = GameManager.instance.transform.Find("GlobalPool/Audio Player Actor(Clone)").gameObject;
            var audioPlayer = new GameObject("Audio Player Actor");
            audioPlayer.SetActive(false);
            var audioSource = audioPlayer.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup =
                FindObjectsOfType<AudioMixerGroup>().FirstOrDefault(group => group.name == "Actors");
            audioPlayer.AddComponent<PlayAudioAndRecycle>().audioSource = audioSource;
            

            foreach (var state in _control.FsmStates)
            {
                foreach (var action in state.Actions)
                {
                    if (action is AudioPlayerOneShotSingle single)
                    {
                        single.audioPlayer.Value = audioPlayer;
                    }
                }
            }
            
            yield return null;
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
