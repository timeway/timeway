using UnityEngine;

namespace Timeway.Gameplay.Player
{
    public class PlayerWallColliderHandler : MonoBehaviour
    {
        private static bool m_IsCollidingWithWall;
        public static bool isCollidingWithWall => m_IsCollidingWithWall;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Wall"))
            {
                m_IsCollidingWithWall = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Wall"))
            {
                m_IsCollidingWithWall = false;
            }
        }
    }
}