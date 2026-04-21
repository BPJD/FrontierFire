using UnityEngine;
using UnityEngine.InputSystem;

public class MoveInputRuntimeDebug : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction jumpAction;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("[MoveRuntimeDebug] PlayerInput not found.");
            enabled = false;
            return;
        }

        moveAction = playerInput.actions.FindAction("Move", false);
        jumpAction = playerInput.actions.FindAction("Jump", false);
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }

        if (jumpAction != null)
        {
            jumpAction.performed += OnJumpPerformed;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }

        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
        }
    }

    private void Update()
    {
        if (moveAction != null)
        {
            Vector2 move = moveAction.ReadValue<Vector2>();
            if (move != Vector2.zero)
            {
                Debug.Log($"[MoveRuntimeDebug] ReadValue Move = {move}");
            }
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[MoveRuntimeDebug] performed Move = {ctx.ReadValue<Vector2>()}");
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[MoveRuntimeDebug] canceled Move = {ctx.ReadValue<Vector2>()}");
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("[MoveRuntimeDebug] performed Jump");
    }
}