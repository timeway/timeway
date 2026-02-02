using Timeway.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Timeway.UI
{
    public class LifeBarHandler : MonoBehaviour
    {
        [SerializeField] private Image m_RedLifeImage;
        [SerializeField] private PlayerController player;

        private void Update()
        {
            m_RedLifeImage.fillAmount = player.Health / player.MAX_LIFE;
        }

        public void LifeIncreaseOrDecrease()
        {
            m_RedLifeImage.fillAmount = player.Health / player.MAX_LIFE;
        }
    }
}
