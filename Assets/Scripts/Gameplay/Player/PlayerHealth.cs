using UnityEngine;
using System;

namespace Timeway.Gameplay.Player
{
    [System.Serializable]
    public class PlayerHealth
    {
        public event Action<float, float> OnHealthChanged;

        [field: SerializeField] public float currentHealth { get; private set; }
        private float maxHealth { get; } = 100f;

        [field: SerializeField] public float Health { get; set; }
        [field: SerializeField] public float healthCapacity { get; set; }

        public void Initialize()
        {
            currentHealth = maxHealth;
            Health = maxHealth;
            Notify();
        }

        public void TakeDamage(float amount, GameObject source)
        {
            currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
            Notify();
        }

        public void TakeHealth(float amount, GameObject source)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            Notify();
        }

        public void RestoreFullHealth()
        {
            currentHealth = maxHealth;
            Health = maxHealth;
            Notify();
        }

        private void Notify()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
