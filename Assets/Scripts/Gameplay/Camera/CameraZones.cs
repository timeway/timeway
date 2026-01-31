using System;
using Timeway.Gameplay.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Timeway.Gameplay.Camera
{
    public class CameraZones : MonoBehaviour
    {
        public event Action<CameraZones> OnEnter;
        public event Action<CameraZones> OnExit;

        [SerializeField] private CinemachineCamera m_CinemachineCamera;
        public CinemachineCamera Camera => m_CinemachineCamera;

        private void Awake()
        {
            m_CinemachineCamera.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out _))
                OnEnter?.Invoke(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out _))
                OnExit?.Invoke(this);
        }
    }
}
