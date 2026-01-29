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
        
        void Start()
        {
        
        }

        void Update()
        {
            GetInput();
        }

        void GetInput()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (m_RedLifeImage.fillAmount > MAX_FILL_AMOUNT)
                {
                    return;
                }
                m_RedLifeImage.fillAmount += (MAX_LIFE - Random.Range(System.MathF.Sqrt(MAX_LIFE), MAX_LIFE)) / MAX_LIFE;
            } else if (Input.GetKeyDown(KeyCode.J))
            {
                if (m_RedLifeImage.fillAmount < MIN_LIFE)
                {
                    return;
                }
                m_RedLifeImage.fillAmount -= MAX_LIFE * Random.Range(System.MathF.Sqrt(MAX_LIFE), MAX_LIFE) / System.MathF.Pow(MAX_LIFE, 2);
            }
        }
    }
}
