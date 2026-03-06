using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UI_LeftRightSelectorClickRouter : MonoBehaviour
{
    [Header("Input Source")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string navigateActionName = "Navigate"; // UI/Navigate

    [Header("Threshold / Repeat")]
    [SerializeField] private float deadzone = 0.6f;
    [SerializeField] private float firstRepeatDelay = 0.30f;
    [SerializeField] private float repeatRate = 0.12f;

    private InputAction _nav;
    private float _nextFireTime;
    private bool _holding;
    private int _holdDir;

    void Awake()
    {
        if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();
        _nav = playerInput && playerInput.actions != null
            ? playerInput.actions.FindAction(navigateActionName, throwIfNotFound: false)
            : null;

        if (_nav == null)
            Debug.LogWarning($"[UI_LeftRightSelectorClickRouter] Navigate action not found: '{navigateActionName}'");
    }

    void OnEnable() => _nav?.Enable();
    void OnDisable() => _nav?.Disable();

    void Update()
    {
        if (_nav == null || EventSystem.current == null) return;

        var go = EventSystem.current.currentSelectedGameObject;
        if (!go) return;

        // 선택된 오브젝트(또는 부모)에 Selector 어댑터가 있어야만 처리
        var selector = go.GetComponentInParent<ILeftRightAdjustable>();
        if (selector == null || !selector.IsInteractable) return;

        Vector2 v = _nav.ReadValue<Vector2>();
        int dir = 0;
        if (v.x <= -deadzone) dir = -1;
        else if (v.x >= deadzone) dir = +1;

        if (dir == 0)
        {
            _holding = false;
            return;
        }

        float now = Time.unscaledTime;

        // 첫 입력은 즉시 1회 실행
        if (!_holding || dir != _holdDir)
        {
            Fire(selector, dir);
            _holding = true;
            _holdDir = dir;
            _nextFireTime = now + firstRepeatDelay;
            return;
        }

        // 홀드 반복
        if (now >= _nextFireTime)
        {
            Fire(selector, dir);
            _nextFireTime = now + repeatRate;
        }
    }

    void Fire(ILeftRightAdjustable selector, int dir)
    {
        if (dir < 0) selector.Prev();
        else selector.Next();
    }
}