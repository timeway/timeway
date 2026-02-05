using System.Collections;
using Timeway.Interfaces;
using Timeway.Input;
using Timeway.Gameplay.Player.UI;
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

        private bool CanMove =>
                !m_IsDead &&
                !m_IsInteracting &&
                !m_OnKnockback &&
                !m_IsAttacking;

        [Header("Reference Fields")]
        [SerializeField] private Rigidbody2D m_Rigidbody2D;
        [SerializeField] private Animator m_Aniamtor;
        [SerializeField] private PlayerSwordController m_PlayerSword;
        [SerializeField] private UIDeathPlayerManager m_UIDeathPlayerHandler;
        [SerializeField] private Transform m_StartPlayerPosition;

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
        private bool m_IsDead;

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
            if (m_IsDead)
            {
                m_MoveInput = Vector2.zero;
                m_InputSystemActions.Player.Disable();
                HandleAnimation();
                return;
            }

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
            if (m_IsDead) return;
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

        private void ResetPlayerState()
        {
            m_MoveInput = Vector2.zero;

            m_IsAttacking = false;
            m_IsAttackTimeEnded = true;
            m_OnKnockback = false;
            m_IsInteracting = false;

            m_Rigidbody2D.linearVelocity = Vector2.zero;
        }
        
        private void Die()
        {
            if (m_IsDead) return;

            m_InputSystemActions.Player.Disable();
            ResetPlayerState();

            m_OnKnockback = false;
            m_IsAttacking = false;
            m_IsInteracting = false;


            m_Rigidbody2D.linearVelocity = Vector2.zero;
            m_MoveInput = Vector2.zero;
            m_UIDeathPlayerHandler.Show();
            m_IsDead = true;
            HandleAnimation();

            StartCoroutine(ComebackPlayerPosition(m_StartPlayerPosition));
        }

        public void TakeDamage(float amount, GameObject other)
        {
            if (m_OnKnockback || m_IsDead)
                return;
            
            m_PlayerHealth.TakeDamage(amount, other);

            foreach (var onDamage in m_PlayerEvents)
            {
                onDamage?.Invoke();
            }

            float deltaX = transform.position.x - other.transform.position.x;
            Vector2 knockDir = deltaX >= 0 ? Vector2.right : Vector2.left;

            float velocityX = m_Rigidbody2D.linearVelocity.x;
            if (Mathf.Abs(velocityX) > 0.01f)
            {
                bool movingTowardsEnemy =
                    (velocityX > 0 && deltaX < 0) ||
                    (velocityX < 0 && deltaX > 0);

                if (movingTowardsEnemy)
                    knockDir *= 1.2f;
            }

            knockDir += Vector2.up;

            KnockbackPlayer(knockDir, 1.5f);

            if (m_PlayerHealth.CurrentHealth <= 0f && !m_IsDead)
            {
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                m_InputSystemActions.Player.Disable();
                m_Rigidbody2D.linearVelocity = Vector2.zero;
                HandleAnimation();
                Die();
            }
        }

        private void KnockbackPlayer(Vector2 direction, float forceMultiplier)
        {
            if (m_OnKnockback || m_IsDead)
                return;
            
            direction.Normalize();

            m_Rigidbody2D.linearVelocity = Vector2.zero;
            m_Rigidbody2D.AddForce(direction * m_KnockbackForce * forceMultiplier, ForceMode2D.Impulse);

            m_OnKnockback = true;
            m_IsOnGround = false;
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

        private IEnumerator ComebackPlayerPosition(Transform m_Transform)
        {
            yield return new WaitForSeconds(0.5f);
            m_IsDead = true;
            yield return new WaitForSeconds(0.6f);
            
            transform.position = m_Transform.position;

            m_MoveInput = Vector2.zero;
            m_Rigidbody2D.linearVelocity = Vector2.zero;

            m_PlayerHealth.Health = MAX_LIFE;
            m_IsDead = false;
            m_UIDeathPlayerHandler.Hide();

            m_InputSystemActions.Player.Enable();
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
            m_Aniamtor.SetBool("Idle", m_Rigidbody2D.linearVelocity.x == 0f && m_IsOnGround); m_Aniamtor.SetBool("Walking", m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround); m_Aniamtor.SetBool("Jumping", m_Rigidbody2D.linearVelocity.y != 0f && !m_IsOnGround); m_Aniamtor.SetBool("Attacking", m_IsAttacking && (m_Rigidbody2D.linearVelocity.x == 0f && m_IsOnGround || m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround || m_Rigidbody2D.linearVelocity.y != 0f && !m_IsOnGround));
            m_Aniamtor.SetBool("Dead", m_PlayerHealth.Health <= 0f && m_IsDead && !m_IsOnGround);
            // if (m_IsDead)
            // {
            //     m_Aniamtor.SetBool("Dead", true);
            //     m_Aniamtor.SetBool("Idle", false);
            //     m_Aniamtor.SetBool("Walking", false);
            //     m_Aniamtor.SetBool("Jumping", false);
            //     m_Aniamtor.SetBool("Attacking", false);
            //     return;
            // }

            // bool isWalking = CanMove && m_MoveInput.x != 0f && m_IsOnGround;

            // m_Aniamtor.SetBool("Dead", false);
            // m_Aniamtor.SetBool("Idle", CanMove && m_MoveInput.x == 0f && m_IsOnGround);
            // m_Aniamtor.SetBool("Walking", isWalking);
            // m_Aniamtor.SetBool("Jumping", m_Rigidbody2D.linearVelocity.y != 0f && !m_IsOnGround);
            // m_Aniamtor.SetBool("Attacking", m_IsAttacking && (m_Rigidbody2D.linearVelocity.x == 0f && m_IsOnGround || m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround || m_Rigidbody2D.linearVelocity.y != 0f && !m_IsOnGround));
        }
    }
}
