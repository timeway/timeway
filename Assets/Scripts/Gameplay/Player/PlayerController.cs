using System.Collections;
using System.Runtime.CompilerServices;
using Timeway.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Timeway.Gameplay.Player
{
    public class PlayerController : MonoBehaviour, IDamageableAndCurable
    {
        [SerializeField] private Rigidbody2D m_Rigidbody2D;
        [SerializeField] private Animator m_Aniamtor;
        [SerializeField] private Transform m_WallColliderTransform;
        [SerializeField] private UnityEvent<GameObject, GameObject, bool>[] m_PlayerEvents;

        public float amount { get; set; }

        private InputSystemActions m_InputSystemActions;
        private Vector2 m_MoveInput;

        private float m_MoveSpeed = 5f;
        private float m_JumpSpeed = 5f;
        private float m_TimeDelay = 1f;
        private float m_AttackTime = 0.4f;
        private bool m_ConditionFlip;
        private bool m_IsOnGround;
        private bool m_IsInteracting;
        private bool m_IsAttacking;
        public bool m_HealthCondiction;

        public InputSystemActions inputActions => m_InputSystemActions;

        public bool isInteracting => m_IsInteracting;

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
            m_InputSystemActions.Player.Health.performed += OnActionsTriggered;
            m_InputSystemActions.Player.Health.canceled += OnActionsTriggered;
            m_InputSystemActions.Player.Attack.started += OnActionsTriggered;
            // m_InputSystemActions.Player.Attack.canceled += OnActionsTriggered;
        }

        private void OnDisable()
        {
            m_InputSystemActions.Player.Move.performed -= OnActionsTriggered;
            m_InputSystemActions.Player.Move.canceled -= OnActionsTriggered;
            m_InputSystemActions.Player.Jump.performed -= OnActionsTriggered;
            m_InputSystemActions.Player.Jump.canceled -= OnActionsTriggered;
            m_InputSystemActions.Player.Health.performed -= OnActionsTriggered;
            m_InputSystemActions.Player.Health.canceled -= OnActionsTriggered;
            m_InputSystemActions.Player.Attack.started -= OnActionsTriggered;
            m_InputSystemActions.Disable();
            m_InputSystemActions.Dispose();
        }

        public void StartInteraction()
        {
            m_IsInteracting = true;
            m_InputSystemActions.Player.Disable();
            m_InputSystemActions.UI.Enable();
            m_MoveInput = Vector2.zero;
            m_Rigidbody2D.linearVelocity = Vector2.zero;
        }

        public void EndInteraction()
        {
            m_IsInteracting = false;
            m_InputSystemActions.UI.Disable();
            m_InputSystemActions.Player.Enable();
        }

        private void FixedUpdate()
        {
            if (m_IsInteracting)
            {
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                HandleAnimation();
                return;
            }

            if (m_IsAttacking)
            {
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                return;
            }

            bool isPressingAndJumping = m_InputSystemActions.Player.Jump.IsPressed() && !m_IsOnGround;

            m_Rigidbody2D.linearVelocity = new Vector2(m_MoveInput.x * m_MoveSpeed, m_Rigidbody2D.linearVelocity.y);
            HandleAnimation();

            if (isPressingAndJumping)
            {
                m_TimeDelay -= Time.fixedDeltaTime;
                if (m_TimeDelay <= 0f && m_InputSystemActions.Player.Jump.IsPressed()) return;
                m_Rigidbody2D.AddForce(Vector2.up * m_JumpSpeed * 1.3f);
                HandleAnimation();
            }
            m_Rigidbody2D.linearVelocity = new Vector2(m_MoveInput.x * m_MoveSpeed,    m_Rigidbody2D.linearVelocity.y);
            if (m_Rigidbody2D.linearVelocity.x != 0)
            {
                m_ConditionFlip = m_Rigidbody2D.linearVelocity.x < 0f;
                transform.localScale = new Vector3(m_ConditionFlip ? -1f : 1f, transform.localScale.y, transform.localScale.z);
                HandleAnimation();
            }
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
            if (ctx.action == m_InputSystemActions.Player.Health)
            {
                if (ctx.performed)
                {
                    m_HealthCondiction = true;
                    TakeDamageOrHealth(Random.Range(amount, 100), gameObject);
                }
                if (ctx.canceled)
                {
                    m_HealthCondiction = false;
                }
            }
            if (ctx.action == m_InputSystemActions.Player.Attack)
            {
                if (ctx.started)
                {
                    m_Rigidbody2D.linearVelocity = Vector2.zero;
                    StartCoroutine(AttackRoutine());
                }
            }
        }

        IEnumerator AttackRoutine()
        {
            m_IsAttacking = true;
            HandleAnimation();
            yield return new WaitForSecondsRealtime(m_AttackTime);
            m_IsAttacking = false;
            HandleAnimation();
        }

        public void TakeDamageOrHealth(float m_Amount, GameObject other)
        {
            foreach (UnityEvent<GameObject, GameObject, bool> @event in m_PlayerEvents)
            {
                @event?.Invoke(other, gameObject, m_HealthCondiction);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                m_IsOnGround = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TakeDamageOrHealth(Random.Range(amount, 100), other.gameObject);
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
            m_Aniamtor.SetBool("Attacking", m_IsAttacking && (m_Rigidbody2D.linearVelocity.x == 0f && m_IsOnGround || m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround || m_Rigidbody2D.linearVelocity.y != 0f && !m_IsOnGround));
        }
    }
}
