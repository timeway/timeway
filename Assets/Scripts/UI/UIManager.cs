using System.Collections;
using Timeway.Gameplay.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Timeway.Input;

namespace Timeway.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Animator m_PanelAnimator;
        [SerializeField] private InputSystemActions input;
        
        private PlayerController player;
        private bool readyForInput = false;

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
            gameObject.SetActive(true);

            readyForInput = false;
            AnimateFadingIn();

            StartCoroutine(WaitAndEnableInput());
        }

        private IEnumerator WaitAndEnableInput()
        {
            yield return new WaitForSeconds(3.2f); // tempo da animação

            readyForInput = true;
        }

        private void OnAnyKey(InputAction.CallbackContext ctx)
        {
            if (!readyForInput) return;

            AnimateFadingOut();
            StartCoroutine(ClosePanel());
        }

        private IEnumerator ClosePanel()
        {
            yield return new WaitForSeconds(0.8f);
            // gameObject.SetActive(false);

            Application.Quit();

            // player.EndInteraction();
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
