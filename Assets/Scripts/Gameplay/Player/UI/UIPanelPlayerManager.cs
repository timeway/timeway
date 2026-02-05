using System.Collections;
using UnityEngine;

namespace Timeway.Gameplay.Player.UI
{
    public class UIPanelPlayerManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator panelAnimator;

        [Header("Timings")]
        [SerializeField] private float fadeInDuration = 3.2f;
        [SerializeField] private float fadeOutDuration = 0.8f;

        private bool isVisible;
        private bool canClose;

        private void Awake()
        {
            SetDefaultState();
        }

        public void Show()
        {
            if (isVisible) return;

            isVisible = true;
            canClose = false;

            gameObject.SetActive(true);
            SetFadeIn();

            StartCoroutine(EnableCloseAfterFadeIn());
        }

        public void Hide()
        {
            if (!isVisible || !canClose) return;

            canClose = false;
            SetFadeOut();

            StartCoroutine(FinishAndClose());
        }

        private IEnumerator EnableCloseAfterFadeIn()
        {
            yield return new WaitForSeconds(fadeInDuration);
            canClose = true;
        }

        private IEnumerator FinishAndClose()
        {
            yield return new WaitForSeconds(fadeOutDuration);

            isVisible = false;
            SetDefaultState();
            // gameObject.SetActive(false);
        }

        private void SetDefaultState()
        {
            panelAnimator.SetBool("Default", true);
            panelAnimator.SetBool("FaddingIn", false);
            panelAnimator.SetBool("FaddingOut", false);
        }

        private void SetFadeIn()
        {
            panelAnimator.SetBool("Default", false);
            panelAnimator.SetBool("FaddingIn", true);
            panelAnimator.SetBool("FaddingOut", false);
        }

        private void SetFadeOut()
        {
            panelAnimator.SetBool("Default", false);
            panelAnimator.SetBool("FaddingIn", false);
            panelAnimator.SetBool("FaddingOut", true);
        }
    }
}
