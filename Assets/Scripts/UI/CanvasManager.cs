using UnityEngine;

namespace Timeway.UI
{
    public class CanvasManager : MonoBehaviour
    {
        private static CanvasManager instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}