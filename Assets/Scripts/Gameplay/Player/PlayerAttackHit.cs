using UnityEngine;
using Timeway.Interfaces;

namespace Timeway.Gameplay.Player
{
    public class PlayerAttackHit : MonoBehaviour
    {
        [SerializeField] private float m_Damage;
        [SerializeField] private GameObject m_Owner;

        public float Damage { get => m_Damage; set => m_Damage = value; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == m_Owner) return;

            if (other.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(m_Damage, m_Owner);
            }
        }
    }
}