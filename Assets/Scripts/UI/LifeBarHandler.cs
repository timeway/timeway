using Timeway.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Timeway.UI
{
    public class LifeBarHandler : MonoBehaviour
    {
        [SerializeField] private Image lifeFill;
        [SerializeField] private PlayerController player;

        private static LifeBarHandler instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            player.onHealthChanged += UpdateLife;
        }

        private void OnDestroy()
        {
            player.onHealthChanged -= UpdateLife;
        }

        private void UpdateLife(float current, float max)
        {
            if (max <= 0f)
            {
                lifeFill.fillAmount = 0f;
                return;
            }

            lifeFill.fillAmount = current / max;
        }
    }
}
