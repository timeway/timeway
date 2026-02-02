using UnityEngine;
using Timeway.Interfaces;
using System;

namespace Timeway.Gameplay.Enemy
{
    public class EnemyLifeBarHandler : MonoBehaviour
    {
        [SerializeField] private GameObject m_EnemyObject;
        [SerializeField] private Transform m_EnemyLifeBarTransform;

        public event Action OnLifeArrivesZero;
        
        public void DecreaseEnemyLife(ref float health, float damage, float maxHealth)
        {
            health -= damage;
            health = Mathf.Clamp(health, 0f, maxHealth);

            float lifePercent = health / maxHealth;

            m_EnemyLifeBarTransform.localScale = new Vector3(
                lifePercent,
                m_EnemyLifeBarTransform.localScale.y,
                m_EnemyLifeBarTransform.localScale.z
            );

            if (health <= 0f)
            {
                OnLifeArrivesZero?.Invoke();
            }
        }

    }
}
