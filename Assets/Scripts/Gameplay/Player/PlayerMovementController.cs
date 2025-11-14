using UnityEngine;
using UnityEngine.InputSystem;

namespace Timeway.Gameplay.Player
{
    public class PlayerMovementController : MonoBehaviour
    {
        public bool isOnGround;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        private InputSystemActions inputSystemActions;
        private float moveSpeed = 5f;
        private float jumpSpeed = 5f;
        private Vector2 moveInput;
        private float timeDelay = 1f;

        void Awake()
        {
            inputSystemActions = new InputSystemActions();
            inputSystemActions.Enable();
        }

        void OnEnable()
        {
            inputSystemActions.Player.Move.performed += OnActionsTriggered;
            inputSystemActions.Player.Move.canceled += OnActionsTriggered;
            inputSystemActions.Player.Jump.performed += OnActionsTriggered;
            inputSystemActions.Player.Jump.canceled += OnActionsTriggered;
        }

        void OnDisable()
        {
            inputSystemActions.Player.Move.performed -= OnActionsTriggered;
            inputSystemActions.Player.Move.canceled -= OnActionsTriggered;
            inputSystemActions.Player.Jump.performed -= OnActionsTriggered;
            inputSystemActions.Player.Jump.canceled -= OnActionsTriggered;
            inputSystemActions.Disable();
            inputSystemActions.Dispose();
        }

        void FixedUpdate()
        {
            if (inputSystemActions.Player.Jump.IsPressed() && !isOnGround)
            {
                timeDelay -= Time.fixedDeltaTime;
                if (timeDelay <= 0f && inputSystemActions.Player.Jump.IsPressed()) return;
                _rigidbody2D.AddForce(Vector2.up * jumpSpeed * 1.3f);
            }
            _rigidbody2D.linearVelocity = new Vector2(moveInput.x * moveSpeed, _rigidbody2D.linearVelocity.y);
        }

        public void OnActionsTriggered(InputAction.CallbackContext ctx)
        {
            if (ctx.action == inputSystemActions.Player.Move)
            {
                if (ctx.performed)
                {
                    moveInput = ctx.ReadValue<Vector2>();
                }
                else if (ctx.canceled)
                {
                    moveInput = Vector2.zero;
                }
            }
            if (ctx.action == inputSystemActions.Player.Jump)
            {
                if (ctx.performed && isOnGround)
                {
                    _rigidbody2D.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
                    isOnGround = false;
                }
                if (ctx.canceled)
                {
                    timeDelay = 1f;
                }
            }
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                isOnGround = true;
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                isOnGround = false;
            }
        }
    }
}
