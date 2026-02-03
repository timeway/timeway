using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Timeway.Interfaces;
using NUnit.Framework;

namespace Timeway.Gameplay.Enemy
{
    public class EnemyController : MonoBehaviour, IDamageable, IDirectionable
    {
        [SerializeField] private GameObject m_EnemyParent;
        [SerializeField] private float m_EnemyMoveSpeed = 5f;
        [SerializeField] private float m_Health;
        [SerializeField] private List<Transform> m_TargetsPosition;
        [SerializeField] private SpriteRenderer m_SpriteRenderer;
        [SerializeField] private BoxCollider2D m_EnemyCollider;
        [SerializeField] private EnemyLifeBarHandler m_EnemyLifeBar;
        [SerializeField] private float m_StopDistance = 0.1f;
        [SerializeField] private float m_Damage;
        [SerializeField] private bool m_IsLookingToRightSide;

        private int m_CurrentTargetIndex = 0;
        private float m_MaxHealth = 100f;
        private bool m_IsVunerable;
        private bool m_CanMove = true;

        public float Damage
        {
            get => m_Damage;
            set => m_Damage = value;
        }

        public bool isLookingToRight
        {
            get => m_IsLookingToRightSide;
        }

        public List<Transform> targetsPosition => m_TargetsPosition;
        public int currentTargetIndex => m_CurrentTargetIndex;


        private void OnEnable()
        {
            m_EnemyLifeBar.OnLifeArrivesZero += LifeArrivesZero;
        }

        private void OnDisable()
        {
            m_EnemyLifeBar.OnLifeArrivesZero -= LifeArrivesZero;
        }

        private void Update()
        {
            if (!m_CanMove) return;
            if (m_TargetsPosition == null || m_TargetsPosition.Count == 0) return;

            MoveCharacter();
        }

        private void MoveCharacter()
        {
            float currentXPosition = transform.position.x;

            Transform target = m_TargetsPosition[m_CurrentTargetIndex];

            transform.position = Vector3.MoveTowards(transform.position, target.position, m_EnemyMoveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < m_StopDistance)
            {
                m_CurrentTargetIndex = (m_CurrentTargetIndex + 1) % m_TargetsPosition.Count;
            }
            float nextXPosition = transform.position.x;
            if (nextXPosition > currentXPosition) m_IsLookingToRightSide = true;
            else if (nextXPosition < currentTargetIndex) m_IsLookingToRightSide = false;
        }

        public void TakeDamage(float m_Amount, GameObject @this)
        {
            if (m_IsVunerable)
                return;
            m_EnemyLifeBar.DecreaseEnemyLife(ref m_Health, m_Amount, m_MaxHealth);
            StartCoroutine(TakeDamageInvunerable());
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Sword"))
            {
                IDamageable iDamageable = other.GetComponentInParent<IDamageable>();
                TakeDamage(iDamageable.Damage, other.gameObject);
            }

            if (other.gameObject.CompareTag("Player"))
            {
                if (other.TryGetComponent<IDamageable>(out var playerDamage))
                {
                    playerDamage.TakeDamage(m_Damage, gameObject);
                }
            }
        }

        private void LifeArrivesZero()
        {
            StartCoroutine(ILifeArrivesZero());
        }

        private IEnumerator ILifeArrivesZero()
        {
            Transform target = m_TargetsPosition[m_CurrentTargetIndex];
            float speed = m_EnemyMoveSpeed;

            while (speed > 0f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target.position,
                    speed * Time.deltaTime
                );

                speed = Mathf.Max(0f, speed - 0.05f);
                yield return null;
            }

            m_CanMove = false;
            StartCoroutine(EnemyDeathRoutine());
        }

        private IEnumerator TakeDamageInvunerable()
        {
            m_IsVunerable = true;

            for (int i = 0; i < 3; i++)
            {
                m_SpriteRenderer.color = new Color(1f, 1f, 1f, 0.1f);
                yield return new WaitForSeconds(0.2f);

                m_SpriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.2f);
            }

            m_IsVunerable = false;
        }

        private IEnumerator EnemyDeathRoutine()
        {
            m_EnemyCollider.enabled = false;

            yield return new WaitForSeconds(1.2f);

            for (float i = 1f; i <= 0; i -= 0.05f)
            {
                m_SpriteRenderer.color = new Color(1, 1, 1, i);
                yield return new WaitForSeconds(0.3f);
            }

            m_EnemyParent.SetActive(false);
        }
    }
}