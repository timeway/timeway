using Timeway.Interfaces;
using Timeway.Gameplay.Player;
using Timeway.UI;
using UnityEngine;

namespace Timeway.Gameplay
{
    public class EndGameInteractable : MonoBehaviour, IAutoInteractable
    {
        private UIManager uiManager;

        private bool used;

        private void Awake()
        {
            if (FindAnyObjectByType<UIManager>() != null)
                uiManager = FindFirstObjectByType<UIManager>();
        }

        public void Interact()
        {
            if (used)
                return;

            used = true;

            if (PlayerController.instance != null)
            {
                PlayerController.instance.StartInteraction();
                uiManager.Show(PlayerController.instance);
            }
        }
    }
}
