using Timeway.Interfaces;
using UnityEngine;

namespace Timeway.Gameplay
{
    public class PlayerObjectColliderTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private string nextScene;
        [SerializeField] private string previousScene;

        public void Interact()
        {
            var loader = LoadSceneManager.instance;
            if (loader == null) return;

            if (!string.IsNullOrEmpty(nextScene))
                loader.SetGoNext(nextScene);

            if (!string.IsNullOrEmpty(previousScene))
                loader.SetGoPrevious(previousScene);

            loader.LoadScene("");
        }
    }
}
