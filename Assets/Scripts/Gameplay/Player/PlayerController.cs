using System.Collections;
using Timeway.Interfaces;
using Timeway.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Timeway.Gameplay.Player
{
    public class PlayerController : MonoBehaviour, IDamageable, ICurable, IDirectionable
    {
        public float MAX_LIFE { get; private set; } = 100f;
        public float MIN_LIFE { get; private set; } = 0f;

        public float Damage
        {
            get => m_PlayerHealth.Damage;
            set => m_PlayerHealth.Damage = value;
        }

        public float Health
        {
            get => m_PlayerHealth.Health;
            set => m_PlayerHealth.Health = value;
        }

        public bool isLookingToRight
        {
            get => m_IsLookingToRightSide;
        }

        public float HealthCapacity
        {
            get => m_PlayerHealth.HealthCapacity;
            set => m_PlayerHealth.HealthCapacity = value;
        }

        [Header("Reference Fields")]
        [SerializeField] private Rigidbody2D m_Rigidbody2D;
        [SerializeField] private Animator m_Aniamtor;
        [SerializeField] private PlayerSwordController m_PlayerSword;

        [Header("Player Event Vector")]
        [SerializeField] private UnityEvent[] m_PlayerEvents;
        [Header("Player Status")]
        [SerializeField] private PlayerHealth m_PlayerHealth;
        [SerializeField] private float m_KnockbackForce = 2f;
        [SerializeField] private float m_KnockbackTime = 0.6f;

        private InputSystemActions m_InputSystemActions;
        private Vector2 m_MoveInput;

        private float m_MoveSpeed = 5f;
        private float m_JumpSpeed = 5f;
        private float m_TimeDelay = 1f;
        private float m_AttackTime = 0.4f;
        private float m_CurrentKnockbackTime;

        private bool m_ConditionFlip;
        private bool m_IsOnGround;
        private bool m_IsInteracting;
        private bool m_IsAttacking;
        private bool m_IsAttackTimeEnded;
        private bool m_OnKnockback;
        private bool m_IsLookingToRightSide;
        private bool m_HealthCondiction;

        public InputSystemActions inputActions => m_InputSystemActions;

        public bool isInteracting => m_IsInteracting;

        private void Awake()
        {
            m_InputSystemActions = new InputSystemActions();
            m_InputSystemActions.Enable();

            m_PlayerHealth.Initialize();
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

        private void Start()
        {
            m_CurrentKnockbackTime = m_KnockbackTime;
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

            if (m_IsAttacking && m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround)
            {
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                HandleAnimation();
                return;
            }

            if (m_IsAttacking && m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround && !m_IsAttackTimeEnded)
            {
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                HandleAnimation();
                return;
            }

            if (m_OnKnockback)
            {
                m_CurrentKnockbackTime -= Time.fixedDeltaTime;

                HandleAnimation();

                if (m_CurrentKnockbackTime <= 0f)
                {
                    m_OnKnockback = false;
                    m_CurrentKnockbackTime = m_KnockbackTime;
                }

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
                m_IsLookingToRightSide = (m_Rigidbody2D.linearVelocity.x > 0f) ? true : false;
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
                    TakeHealth(m_PlayerHealth.HealthCapacity, gameObject);
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
                    HandleAnimation();
                    StartCoroutine(AttackRoutine());
                }
            }
        }

        public void TakeDamage(float amount, GameObject other)
        {
            m_PlayerHealth.TakeDamage(amount, other);

            foreach (var onDamage in m_PlayerEvents)
            {
                onDamage?.Invoke();
            }

            if (Mathf.Abs(m_Rigidbody2D.linearVelocity.x) < 0.01f)
            {
                if (other.TryGetComponent<IDirectionable>(out var enemyDirection))
                {
                    KnockbackPlayer(m_IsLookingToRightSide == enemyDirection.isLookingToRight ? (Vector2)transform.localScale + Vector2.up : new Vector2(transform.localScale.x * -1f, 1) + Vector2.up, 1.5f);
                }
            }
            else if (m_IsLookingToRightSide && m_Rigidbody2D.linearVelocity.x != 0f)
            {
                if (m_Rigidbody2D.linearVelocity.x > 0f)
                    KnockbackPlayer(Vector2.left + Vector2.up, 1f);
                else if (m_Rigidbody2D.linearVelocity.x < 0f)
                    KnockbackPlayer(Vector2.right + Vector2.up, 1f);
            }
        }

        private void KnockbackPlayer(Vector2 direction, float forceMultiplier)
        {
            direction.Normalize();

            m_Rigidbody2D.linearVelocity = Vector2.zero;
            m_Rigidbody2D.AddForce(direction * m_KnockbackForce * forceMultiplier, ForceMode2D.Impulse);

            m_OnKnockback = true;
            m_CurrentKnockbackTime = m_KnockbackTime;
        }

        public void TakeHealth(float amount, GameObject other)
        {
            if (!m_HealthCondiction) return;

            m_PlayerHealth.TakeHealth(amount, other);

            foreach (var onHeal in m_PlayerEvents)
            {
                onHeal?.Invoke();
            }
        }

        IEnumerator AttackRoutine()
        {
            m_IsAttacking = true;
            m_IsAttackTimeEnded = false;

            m_PlayerSword.SwordAttack(m_IsAttacking, m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround);
            HandleAnimation();
            
            yield return new WaitForSecondsRealtime(m_AttackTime);
            
            m_IsAttacking = false;
            m_PlayerSword.SwordAttack(m_IsAttacking, m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround);

            yield return new WaitForSecondsRealtime(1.3f);
            m_IsAttackTimeEnded = true;
            HandleAnimation();
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
            m_Aniamtor.SetBool("Attacking", m_IsAttacking && (m_Rigidbody2D.linearVelocity.x == 0f && m_IsOnGround || m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround || m_Rigidbody2D.linearVelocity.y != 0f && !m_IsOnGround));
        }
    }
}
