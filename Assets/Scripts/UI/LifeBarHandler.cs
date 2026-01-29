using UnityEngine;
using UnityEngine.UI;

namespace Timeway
{
    public class LifeBarHandler : MonoBehaviour
    {
        public Image m_RedLifeImage;
        private const float MAX_LIFE = 100f;
        private const float MIN_LIFE = 0f;
        private const float MAX_FILL_AMOUNT = 1f;

        public void LifeBarDecreaseOrIncrease(GameObject other, GameObject @this)
        {
            if (m_RedLifeImage.fillAmount > MAX_FILL_AMOUNT)
            {
                return;
            }

            if (m_RedLifeImage.fillAmount < MIN_LIFE)
            {
                return;
            }

            if (other.CompareTag("Enemy") && @this.CompareTag("Enemy"))
            {
                return;
            }

            if (other.CompareTag("Enemy") && @this.CompareTag("Player"))
            {
                m_RedLifeImage.fillAmount -= MAX_LIFE * Random.Range(System.MathF.Sqrt(MAX_LIFE), MAX_LIFE) / System.MathF.Pow(MAX_LIFE, 2);
                return;
            }

            m_RedLifeImage.fillAmount += (MAX_LIFE - Random.Range(System.MathF.Sqrt(MAX_LIFE), MAX_LIFE)) / MAX_LIFE;
        }
    }
}
