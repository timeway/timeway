using System.Collections.Generic;
using Timeway.Gameplay.Player;
using UnityEngine;

namespace Timeway.Gameplay.Camera
{
    public class CameraManagerSystem : MonoBehaviour
    {
        [SerializeField] private PlayerController m_Player;
        [SerializeField] private List<CameraZones> m_Zones;

        private readonly Stack<CameraZones> m_ZoneStack = new();
        private CameraZones m_Current;

        private void Start()
        {
            foreach (var zone in m_Zones)
            {
                zone.OnEnter += EnterZone;
                zone.OnExit += ExitZone;
            }
        }

        private void EnterZone(CameraZones zone)
        {
            if (m_ZoneStack.Contains(zone))
                return;

            m_ZoneStack.Push(zone);
            UpdateCamera();
        }

        private void ExitZone(CameraZones zone)
        {
            if (!m_ZoneStack.Contains(zone))
                return;

            var temp = new Stack<CameraZones>();

            while (m_ZoneStack.Peek() != zone)
                temp.Push(m_ZoneStack.Pop());

            m_ZoneStack.Pop();

            while (temp.Count > 0)
                m_ZoneStack.Push(temp.Pop());

            UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (m_Current != null)
                m_Current.Camera.enabled = false;

            if (m_ZoneStack.Count == 0)
                return;

            m_Current = m_ZoneStack.Peek();
            m_Current.Camera.enabled = true;
            m_Current.Camera.Follow = m_Player.transform;
        }
    }
}
