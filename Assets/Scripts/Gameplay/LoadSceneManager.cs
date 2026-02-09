using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Timeway.Gameplay.Player.UI;
using UnityEditor;
using Timeway.Gameplay.Player;
using Unity.VisualScripting;

namespace Timeway.Gameplay
{
    public class LoadSceneManager : MonoBehaviour
    {
        [Header("Persistent Objects")]
        // [SerializeField] private GameObject m_Player;
        [SerializeField] private GameObject m_Canvas;
        [SerializeField] private GameObject m_UILifeBarManager;
        [SerializeField] private GameObject m_CameraManager;
        [SerializeField] private GameObject m_VirtualCamera;
        [SerializeField] private UIPanelPlayerManager m_TransitionPanel;

        [Header("Scenes")]
        [SerializeField] private string m_PreviousScene;
        [SerializeField] private string m_NextScene;

        private static bool m_Initialized;

        private bool m_IsGoingNext;
        private bool m_IsGoingPrevious;

        /// <summary>
        /// A short variable of <b>Next Scene</b> in string value
        /// </summary>
        public string nextScene => m_NextScene;

        public static LoadSceneManager instance { get; private set; }

        public bool isTransitionLocked { get; private set; }
        public PlayerController player;

        private void Awake()
        {
            if (m_Initialized)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            m_Initialized = true;

            SceneManager.sceneLoaded += OnSceneLoaded;

            // 👇 pega o start da PRIMEIRA cena
            StartCoroutine(InitFirstScene());
        }

        private IEnumerator InitFirstScene()
        {
            yield return null; // espera 1 frame
            SetStartPositionFromScene();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetStartPositionFromScene();
        }

        private void SetStartPositionFromScene()
        {
            var startPos = FindFirstObjectByType<StartPositionGameObjectManager>();

            if (startPos == null)
            {
                return;
            }

            if (PlayerController.instance == null)
            {
                return;
            }

            PlayerController.instance.SetvalueToStartPositionObject(startPos.transform);
        }


        public void UnlockTransition()
        {
            isTransitionLocked = false;

            var startPos = FindFirstObjectByType<StartPositionGameObjectManager>();
            if (startPos != null && player != null)
                player.SetvalueToStartPositionObject(startPos.transform);
        }

        public void LoadScene(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                StartCoroutine(LoadRoutine(sceneName));
                return;
            }

            TryLoadByFlow();
        }

        public void SetGoNext(string scene)
        {
            m_NextScene = scene;
            m_PreviousScene = "";
            m_IsGoingNext = true;
            m_IsGoingPrevious = false;
        }

        public void SetGoPrevious(string scene)
        {
            m_PreviousScene = scene;
            m_NextScene = "";
            m_IsGoingPrevious = true;
            m_IsGoingNext = false;
        }

        private void ResetFlow()
        {
            m_IsGoingNext = false;
            m_IsGoingPrevious = false;
        }

        private void TryLoadByFlow()
        {
            if (!m_IsGoingNext && !m_IsGoingPrevious)
                return;

            if (m_IsGoingNext && !m_IsGoingPrevious)
            {
                if (string.IsNullOrEmpty(m_NextScene))
                    return;

                StartCoroutine(LoadRoutine(m_NextScene));
                return;
            }

            if (m_IsGoingPrevious && !m_IsGoingNext)
            {
                if (string.IsNullOrEmpty(m_PreviousScene))
                    return;

                StartCoroutine(LoadRoutine(m_PreviousScene));
            }
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            ResetFlow();

            m_TransitionPanel.Show();
            yield return new WaitForSeconds(0.5f);

            yield return SceneManager.LoadSceneAsync(sceneName);

            yield return new WaitForSeconds(0.2f);
            m_TransitionPanel.Hide();
        }
    }
}
