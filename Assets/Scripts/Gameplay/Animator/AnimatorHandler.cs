using System.Collections.Generic;
using Timeway.Interfaces;
using UnityEngine;

namespace Timeway.Gameplay.AnimatorHanlder
{
    public class AnimatorHandlerController : MonoBehaviour, ICharacterBoolAnimatable
    {
        public static AnimatorHandlerController instance { get; private set; }
        // private Dictionary<int, string> m_HashtoStringMap = new Dictionary<int, string>();
        [SerializeField] private Animator m_Animator;
        // private readonly string m_IdleParamString = "Idle";
        // private readonly string m_RunParamString = "Walking";
        // private readonly string m_JumpParamString = "Jumping";

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void Start()
        {
            // m_HashtoStringMap.Add(Animator.StringToHash(m_IdleParamString), m_IdleParamString);
            // m_HashtoStringMap.Add(Animator.StringToHash(m_RunParamString), m_IdleParamString);
            // m_HashtoStringMap.Add(Animator.StringToHash(m_JumpParamString), m_JumpParamString);
        }

        public void HandleIdleAnimation()
        {
            // m_Animator
        }
        public void HandleWalkingAnimation()
        {
            
        }
        public void HandleJumpingAnimation()
        {
            
        }

        public void AnimatorPlay(string paramName, bool condition)
        {
            m_Animator.SetBool(paramName, condition);
        }
    }
}
