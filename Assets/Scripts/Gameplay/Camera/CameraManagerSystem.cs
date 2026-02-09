using System;
using System.Collections.Generic;
using Timeway.Gameplay.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Timeway.Gameplay.Camera
{
    public class CameraManagerSystem : MonoBehaviour
    {
        [SerializeField] private List<CameraZones> m_Zones;

        private PlayerController m_Player;
        private readonly Stack<CameraZones> m_ZoneStack = new();
        private CameraZones m_Current;
        
        private static CameraManagerSystem instance;

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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            m_Player = FindFirstObjectByType<PlayerController>();
            m_ZoneStack.Clear();
            m_Current = null;

            m_Zones = new List<CameraZones>(FindObjectsByType<CameraZones>(FindObjectsSortMode.None));

            foreach (var zone in m_Zones)
            {
                zone.OnEnter += EnterZone;
                zone.OnExit += ExitZone;
            }
        }

        // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        // {
        //     m_Player = FindFirstObjectByType<PlayerController>();

        //     foreach (var zone in m_Zones)
        //     {
        //         zone.OnEnter += EnterZone;
        //         zone.OnExit += ExitZone;
        //     }
        // }

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
            try {
                if (m_Player == null)
                    return;

                if (m_Current != null)
                    if (m_Current.Camera != null) m_Current.Camera.enabled = false;

                if (m_ZoneStack.Count == 0)
                    return;

                m_Current = m_ZoneStack.Peek();
                m_Current.Camera.enabled = true;
                m_Current.Camera.Follow = m_Player.transform;
            } catch (Exception e)
            {
                print($"{e.StackTrace}: {e.Message}");
            }
        }
    }
}
