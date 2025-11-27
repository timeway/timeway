using System;
using Timeway.Gameplay.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Timeway.Gameplay.Camera
{
    public class CameraZones : MonoBehaviour
    {
        public Action<CameraZones> onColliderTrigger;

        [SerializeField] private CinemachineCamera cinemachineCamera;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerMovementController>(out var ctx))
            {
                onColliderTrigger?.Invoke(this);
                print($"{other.gameObject.name} esta colidindo com {gameObject.name}");
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
