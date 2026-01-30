using Timeway.Gameplay.Player;
using Timeway.UI;
using UnityEngine;

namespace Timeway.Gameplay.Interactives
{
    public class DoorInteract : MonoBehaviour
    {
        // public static event System.Action<PlayerMovementController> onDoorEntered;
        [SerializeField] private UIManager m_UIManager; 

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                player.StartInteraction();
                m_UIManager.Show(player); // painel de UI
            }
        }
    }
}
