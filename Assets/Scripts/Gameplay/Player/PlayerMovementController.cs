using System.Runtime.CompilerServices;
using Timeway.Gameplay.AnimatorHanlder;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Timeway.Gameplay.Player
{
    public class PlayerMovementController : MonoBehaviour
    {
        private bool m_IsOnGround;
        [SerializeField] private Rigidbody2D m_Rigidbody2D;
        [SerializeField] private SpriteRenderer m_SpriteRenderer;
        [SerializeField] private Animator m_Aniamtor;
        [SerializeField] private Transform m_TransformChildCollider;
        
        private InputSystemActions m_InputSystemActions;
        private float m_MoveSpeed = 5f;
        private float m_JumpSpeed = 5f;
        private Vector2 m_MoveInput;
        private float m_TimeDelay = 1f;
        private bool m_ConditionFlip;

        private void Awake()
        {
            m_InputSystemActions = new InputSystemActions();
            m_InputSystemActions.Enable();
        }

        private void OnEnable()
        {
            m_InputSystemActions.Player.Move.performed += OnActionsTriggered;
            m_InputSystemActions.Player.Move.canceled += OnActionsTriggered;
            m_InputSystemActions.Player.Jump.performed += OnActionsTriggered;
            m_InputSystemActions.Player.Jump.canceled += OnActionsTriggered;
        }

        private void OnDisable()
        {
            m_InputSystemActions.Player.Move.performed -= OnActionsTriggered;
            m_InputSystemActions.Player.Move.canceled -= OnActionsTriggered;
            m_InputSystemActions.Player.Jump.performed -= OnActionsTriggered;
            m_InputSystemActions.Player.Jump.canceled -= OnActionsTriggered;
            m_InputSystemActions.Disable();
            m_InputSystemActions.Dispose();
        }

        private void FixedUpdate()
        {
            if (m_InputSystemActions.Player.Jump.IsPressed() && !m_IsOnGround)
            {
                m_TimeDelay -= Time.fixedDeltaTime;
                if (m_TimeDelay <= 0f && m_InputSystemActions.Player.Jump.IsPressed()) return;
                m_Rigidbody2D.AddForce(Vector2.up * m_JumpSpeed * 1.3f);
            }
            m_Rigidbody2D.linearVelocity = new Vector2(m_MoveInput.x * m_MoveSpeed,    m_Rigidbody2D.linearVelocity.y);
            if (m_Rigidbody2D.linearVelocity.x != 0)
            {
                m_ConditionFlip = m_Rigidbody2D.linearVelocity.x < 0f;
                m_SpriteRenderer.flipX = m_ConditionFlip;
                m_TransformChildCollider.localScale = new Vector3(m_ConditionFlip ? -1f : 1f, m_TransformChildCollider.localScale.y, m_TransformChildCollider.localScale.z);
            }
            HandleAnimation();
        }

        private void OnActionsTriggered(InputAction.CallbackContext ctx)
        {
            if (ctx.action == m_InputSystemActions.Player.Move)
            {
                if (ctx.performed)
                {
                    m_MoveInput = ctx.ReadValue<Vector2>();
                }
                else if (ctx.canceled)
                {
                    m_MoveInput = Vector2.zero;
                }
            }
            if (ctx.action == m_InputSystemActions.Player.Jump)
            {
                if (ctx.performed && m_IsOnGround)
                {
                    m_Rigidbody2D.AddForce(Vector2.up * m_JumpSpeed, ForceMode2D.Impulse);
                    m_IsOnGround = false;
                }
                if (ctx.canceled)
                {
                    m_TimeDelay = 1f;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                m_IsOnGround = true;
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                m_IsOnGround = false;
            }
        }

        private void HandleAnimation()
        {
            m_Aniamtor.SetBool("Idle", m_Rigidbody2D.linearVelocity.x == 0f && m_IsOnGround);
            m_Aniamtor.SetBool("Walking", m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround);
            m_Aniamtor.SetBool("Jumping", m_Rigidbody2D.linearVelocity.y != 0f && !m_IsOnGround);
        }
    }
}
