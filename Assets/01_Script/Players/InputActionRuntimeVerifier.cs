using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputActionRuntimeVerifier : MonoBehaviour
{
    [SerializeField] private string actionName = "Jump";

    private PlayerInput _playerInput;
    private InputAction _action;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        MainUI_KeyMapLoader.Instance.ApplyTo(GetComponent<PlayerInput>().actions);

        // PlayerInput이 실제로 사용하는 actions에서 액션을 가져온다 (중요)
        _action = _playerInput.actions.FindAction(actionName, throwIfNotFound: false);

        if (_action == null)
        {
            Debug.LogError($"[Verifier] Action not found: {actionName}");
            enabled = false;
            return;
        }

        _action.performed += OnPerformed;
        _action.canceled += OnCanceled;
    }

    private void OnDestroy()
    {
        if (_action != null)
        {
            _action.performed -= OnPerformed;
            _action.canceled -= OnCanceled;
        }
    }

    private void OnPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[Verifier] {actionName} PERFORMED | control={ctx.control?.path} | device={ctx.control?.device?.displayName}");
    }

    private void OnCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[Verifier] {actionName} CANCELED");
    }
}
