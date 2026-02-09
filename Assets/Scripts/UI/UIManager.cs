using System.Collections;
using Timeway.Gameplay.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Timeway.Input;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Timeway.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Animator m_PanelAnimator;
        [SerializeField] private InputSystemActions input;

        private PlayerController player;
        private bool readyForInput;

        private void Awake()
        {
            input = new InputSystemActions();
        }

        private void OnEnable()
        {
            input.UI.AnyKey.performed += OnAnyKey;
            input.UI.Enable();
        }

        private void OnDisable()
        {
            input.UI.AnyKey.performed -= OnAnyKey;
            input.UI.Disable();
        }

        public void Show(PlayerController player)
        {
            this.player = player;

            readyForInput = false;
            AnimateFadingIn();

            StartCoroutine(WaitAndEnableInput());
        }

        private IEnumerator WaitAndEnableInput()
        {
            yield return new WaitForSeconds(3.2f);
            readyForInput = true;
        }

        private void OnAnyKey(InputAction.CallbackContext ctx)
        {
            if (!readyForInput)
                return;

            readyForInput = false;

            AnimateFadingOut();
            StartCoroutine(ClosePanel());
        }

        private IEnumerator ClosePanel()
        {
            yield return new WaitForSeconds(2f);

#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        private void AnimateFadingIn()
        {
            m_PanelAnimator.SetBool("Default", false);
            m_PanelAnimator.SetBool("FaddingIn", true);
            m_PanelAnimator.SetBool("FaddingOut", false);
        }

        private void AnimateFadingOut()
        {
            m_PanelAnimator.SetBool("Default", false);
            m_PanelAnimator.SetBool("FaddingIn", false);
            m_PanelAnimator.SetBool("FaddingOut", true);
        }
    }
}
