using System.Collections.Generic;
using Timeway.Gameplay.Player;
using UnityEngine;

namespace Timeway.Gameplay.Camera
{
    public class CameraManagerSystem : MonoBehaviour
    {
        [SerializeField] private List<CameraZones> m_Zones;
        [SerializeField] private PlayerMovementController m_PlayerPosition; 

        [field: SerializeField] private CameraZones m_CurrentZone;

        public CameraZones currentZone
        { 
            get => m_CurrentZone;
            set
            {
                m_CurrentZone?.DisableCamera();
                m_CurrentZone = value;
                m_CurrentZone?.EnableCamera(m_PlayerPosition.transform);
            }
        }

        private void Start()
        {
            m_Zones.ForEach(zone => zone.onColliderTrigger += HandleCamera);
        }

        private void OnDestroy()
        {
            m_Zones.ForEach(zone => zone.onColliderTrigger -= HandleCamera);
        }

        private void HandleCamera(CameraZones zones)
        {
            currentZone = zones;
        }
    }
}
