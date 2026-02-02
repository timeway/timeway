using UnityEngine;

namespace Timeway.Gameplay.Player
{
    public class PlayerSwordController : MonoBehaviour
    {
        [SerializeField] private Transform m_SwordPosition;
        [SerializeField] private CapsuleCollider2D m_SwordCollider;

        public void SwordAttack(bool m_IsAttacking, bool m_IsWalking)
        {
            if (m_IsAttacking && !m_IsWalking) {
                m_SwordCollider.enabled = true;
            }
            else
            {
                m_SwordCollider.enabled = false;
            }
        }
    }
}
