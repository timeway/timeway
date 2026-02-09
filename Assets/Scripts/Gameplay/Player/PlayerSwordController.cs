using UnityEngine;

namespace Timeway.Gameplay.Player
{
    public class PlayerSwordController : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider2D m_SwordCollider;

        public void EnableSword()
        {
            m_SwordCollider.enabled = true;
        }

        public void DisableSword()
        {
            m_SwordCollider.enabled = false;
        }
    }
}
