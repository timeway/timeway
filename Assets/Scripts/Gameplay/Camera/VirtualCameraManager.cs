using UnityEngine;

namespace Timeway.Gameplay.Camera
{
    public class VirtualCameraManager : MonoBehaviour
    {
        private static VirtualCameraManager instance;

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