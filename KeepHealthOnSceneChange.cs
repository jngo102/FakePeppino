using Modding;
using UnityEngine;

namespace FakePeppino
{
    internal class KeepHealthOnSceneChange : MonoBehaviour
    {
        private int _currentHealthBlue;
        private int _currentHealth;
        private int _currentJoniHealth;

        private void Awake()
        {
            _currentHealth = PlayerData.instance.health;
            _currentHealthBlue = PlayerData.instance.healthBlue;
            _currentJoniHealth = PlayerData.instance.joniHealthBlue;

            ModHooks.TakeDamageHook += OnTakeDamage;
        }

        private int OnTakeDamage(ref int hazardType, int damage)
        {
            _currentHealth = PlayerData.instance.health;
            _currentHealthBlue = PlayerData.instance.healthBlue;
            _currentJoniHealth = PlayerData.instance.joniHealthBlue;
            return damage;
        }

        public void ResetHealth()
        {
            PlayerData.instance.health = _currentHealth;
            PlayerData.instance.healthBlue = _currentHealthBlue;
            PlayerData.instance.joniHealthBlue = _currentJoniHealth;

            var healthParent = GameCameras.instance.hudCanvas.transform.Find("Health");
            for (int healthNum = 1; healthNum <= 11; healthNum++)
            {
                var healthDisplay = healthParent.Find($"Health {healthNum}").gameObject.LocateMyFSM("health_display");
                healthDisplay.SendEvent("HERO DAMAGED");
            }
        }
    }
}
