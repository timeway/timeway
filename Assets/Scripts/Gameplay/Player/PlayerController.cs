using System.Collections;
using Timeway.Interfaces;
using Timeway.Input;
using Timeway.Gameplay.Player.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System;

namespace Timeway.Gameplay.Player
{
    public class PlayerController : MonoBehaviour, IDamageable, ICurable, IDirectionable
    {
        #region events
        public event Action<float, float> onHealthChanged
        {
            add => m_PlayerHealth.OnHealthChanged += value;
            remove => m_PlayerHealth.OnHealthChanged -= value;
        }
        #endregion

        #region variables
        public static PlayerController instance;
        public float MAX_LIFE { get; private set; } = 100f;
        public float MIN_LIFE { get; private set; } = 0f;

        public float Damage
        {
            get => m_PlayerAttackHit.Damage;
            set => m_PlayerAttackHit.Damage = value;
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
            get => m_PlayerHealth.healthCapacity;
            set => m_PlayerHealth.healthCapacity = value;
        }

        [Header("Reference Fields")]
        [SerializeField] private Rigidbody2D m_Rigidbody2D;
        [SerializeField] private Animator m_Aniamtor;
        [SerializeField] private PlayerSwordController m_PlayerSword;
        [SerializeField] private PlayerAttackHit m_PlayerAttackHit;
        [SerializeField] private UIPanelPlayerManager m_UIDeathPlayerHandler;
        [SerializeField] private SpriteRenderer m_SpriteRenderer;
        [SerializeField] private CapsuleCollider2D m_PlayerCollider;

        [Header("Player Event Vector")]
        [SerializeField] private UnityEvent[] m_PlayerEvents;
        [Header("Player Status")]
        [SerializeField] private PlayerHealth m_PlayerHealth;
        [SerializeField] private float m_KnockbackForce = 2f;
        [SerializeField] private float m_KnockbackTime = 0.6f;

        [Header("Layers")]
        [SerializeField] private LayerMask m_NothingLayer;
        [SerializeField] private LayerMask m_EnemyLayer;

        private InputSystemActions m_InputSystemActions;
        private Vector2 m_MoveInput;
        private Transform m_StartPlayerPosition;
        private IInteractable m_CurrentInteractable;

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
        private bool m_IsVunerable;
        private bool m_IsInteractingWithDoor;
        #endregion

        #region readonly
        public InputSystemActions inputActions => m_InputSystemActions;

        public bool isInteracting => m_IsInteracting;
        public Rigidbody2D physicsController2D => m_Rigidbody2D;
        public bool isMoving => m_Rigidbody2D.linearVelocityX != 0;
        public bool isInteractingWithDoor => m_IsInteractingWithDoor;
        public bool canInteract => !m_IsDead && !m_IsInteracting && !m_IsAttacking && m_IsOnGround;
        #endregion

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            
            m_InputSystemActions = new InputSystemActions();
            m_InputSystemActions.Enable();

            m_PlayerHealth.Initialize();
        }

        private void OnEnable()
        {
            m_InputSystemActions.Player.Move.performed     += OnActionsTriggered;
            m_InputSystemActions.Player.Move.canceled      += OnActionsTriggered;
            m_InputSystemActions.Player.Jump.performed     += OnActionsTriggered;
            m_InputSystemActions.Player.Jump.canceled      += OnActionsTriggered;
            m_InputSystemActions.Player.Health.performed   += OnActionsTriggered;
            m_InputSystemActions.Player.Health.canceled    += OnActionsTriggered;
            m_InputSystemActions.Player.Attack.started     += OnActionsTriggered;
            m_InputSystemActions.Player.Interact.started += OnActionsTriggered;
        }

        private void OnDisable()
        {
            m_InputSystemActions.Player.Move.performed     -= OnActionsTriggered;
            m_InputSystemActions.Player.Move.canceled      -= OnActionsTriggered;
            m_InputSystemActions.Player.Jump.performed     -= OnActionsTriggered;
            m_InputSystemActions.Player.Jump.canceled      -= OnActionsTriggered;
            m_InputSystemActions.Player.Health.performed   -= OnActionsTriggered;
            m_InputSystemActions.Player.Health.canceled    -= OnActionsTriggered;
            m_InputSystemActions.Player.Attack.started     -= OnActionsTriggered;
            m_InputSystemActions.Player.Interact.started   -= OnActionsTriggered;
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
            m_MoveInput = new Vector2(0f, m_MoveInput.y);
            m_Rigidbody2D.linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
        }

        public void EndInteraction()
        {
            m_IsInteracting = false;
            m_InputSystemActions.UI.Disable();
            m_InputSystemActions.Player.Enable();
        }

        public void SetvalueToStartPositionObject(Transform m_Arg)
        {
            m_StartPlayerPosition = m_Arg;
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
                m_Rigidbody2D.linearVelocity = new Vector2(0f, m_Rigidbody2D.linearVelocity.y);
                HandleAnimation();
                return;
            }

            if (m_IsAttacking && m_IsOnGround)
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
                    TakeHealth(m_PlayerHealth.healthCapacity, gameObject);
                }
                if (ctx.canceled)
                {
                    m_HealthCondiction = false;
                }
            }
            if (ctx.action == m_InputSystemActions.Player.Attack && ctx.started)
            {
                StartCoroutine(AttackRoutine());
            }
            if (ctx.action == m_InputSystemActions.Player.Interact && ctx.started)
            {
                if (m_CurrentInteractable == null)
                    return;

                if (!canInteract)
                    return;

                m_CurrentInteractable.Interact();
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

        private IEnumerator TakeDamageInvunerable()
        {
            m_IsVunerable = true;
            m_Rigidbody2D.excludeLayers = m_EnemyLayer;

            for (int i = 0; i < 3; i++)
            {
                m_SpriteRenderer.color = new Color(1f, 1f, 1f, 0.1f);
                yield return new WaitForSeconds(0.2f);

                m_SpriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.2f);
            }
            m_IsVunerable = false;
            m_Rigidbody2D.excludeLayers = m_NothingLayer;
        }

        public void TakeDamage(float amount, GameObject other)
        {
            if (m_OnKnockback || m_IsDead || m_IsVunerable)
                return;
            
            m_PlayerHealth.TakeDamage(amount, other);

            foreach (var onDamage in m_PlayerEvents)
            {
                onDamage?.Invoke();
            }

            StartCoroutine(TakeDamageInvunerable());

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

            if (m_PlayerHealth.currentHealth <= 0f && !m_IsDead)
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
            if (m_IsAttacking) yield break;

            m_IsAttacking = true;
            m_IsAttackTimeEnded = false;

            m_PlayerSword.EnableSword();
            HandleAnimation();

            yield return new WaitForSeconds(m_AttackTime);

            m_PlayerSword.DisableSword();
            m_IsAttacking = false;

            yield return new WaitForSeconds(0.2f);
            m_IsAttackTimeEnded = true;
            HandleAnimation();
        }

        private IEnumerator ComebackPlayerPosition(Transform m_Transform)
        {
            if (m_Transform == null)
            {
                yield break;
            }

            yield return new WaitForSeconds(1.1f);

            transform.position = m_Transform.position;

            m_MoveInput = Vector2.zero;
            m_Rigidbody2D.linearVelocity = Vector2.zero;

            m_IsOnGround = true;

            m_PlayerHealth.RestoreFullHealth();
            m_UIDeathPlayerHandler.Hide();

            m_IsDead = false;
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<IAutoInteractable>(out var autoInteractable))
            {
                autoInteractable.Interact();
                return;
            }

            if (other.TryGetComponent<IInteractable>(out var interactable))
            {
                m_CurrentInteractable = interactable;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<IInteractable>(out var interactable) && m_CurrentInteractable == interactable)
            {
                m_CurrentInteractable = null;
            }
        }

        private void HandleAnimation()
        {
            m_Aniamtor.SetBool("Idle", m_Rigidbody2D.linearVelocity.x == 0f && m_IsOnGround);
            m_Aniamtor.SetBool("Walking", m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround);
            m_Aniamtor.SetBool("Jumping", m_Rigidbody2D.linearVelocity.y != 0f && !m_IsOnGround);
            m_Aniamtor.SetBool("Attacking", m_IsAttacking && (m_Rigidbody2D.linearVelocity.x == 0f && m_IsOnGround || m_Rigidbody2D.linearVelocity.x != 0f && m_IsOnGround || m_Rigidbody2D.linearVelocity.y != 0f && !m_IsOnGround));
            m_Aniamtor.SetBool("Dead", m_PlayerHealth.Health <= 0f && m_IsDead);
        }
    }
}
