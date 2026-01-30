using System;
using Timeway.Gameplay.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Timeway.Gameplay.Camera
{
    public class CameraZones : MonoBehaviour
    {
        public event Action<CameraZones> onColliderTrigger;

        [SerializeField] private CinemachineCamera cinemachineCamera;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out var ctx))
            {
                onColliderTrigger?.Invoke(this);
            }
        }

        public void DisableCamera()
        {
            cinemachineCamera.enabled = false;
        }

        public void EnableCamera(Transform target)
        {
            cinemachineCamera.enabled = true;
            cinemachineCamera.Follow = target;
        }
    }
}
