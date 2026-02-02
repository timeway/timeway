using UnityEngine;
using Timeway.Interfaces;

namespace Timeway.Gameplay.Player
{
    [System.Serializable]
    public class PlayerHealth : IDamageable, ICurable
    {
        [field: SerializeField] public float CurrentHealth { get; private set; }
        public float MaxHealth;
        [property: SerializeField] public float Health { get => CurrentHealth; set => CurrentHealth = value; }
        [field: SerializeField] public float HealthCapacity { get; set; }
        [field: SerializeField] public float Damage { get; set; }

        public void Initialize()
        {
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(float amount, GameObject source)
        {
            CurrentHealth -= amount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
        }

        public void TakeHealth(float amount, GameObject source)
        {
            CurrentHealth += amount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
        }
    }
}