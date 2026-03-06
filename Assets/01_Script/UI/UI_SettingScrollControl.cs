using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_SettingScrollControl : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Action Name in UI Map")]
    [SerializeField] private string scrollActionName = "ScrollWheel"; // 너 맵 이름 그대로

    [Header("Scroll Settings")]
    [SerializeField] private float speed = 0.9f;
    [SerializeField] private float deadzone = 0.25f;
    [SerializeField] private bool invertY = true;

    [SerializeField] private ScrollRect fallbackScrollRect;

    private InputAction _scrollAction;
    private ScrollRect _cached;

    void Awake()
    {
        if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();
        CacheAction();
    }

    void OnEnable()
    {
        CacheAction();
        _scrollAction?.Enable();
    }

    void OnDisable()
    {
        _scrollAction?.Disable();
    }

    void CacheAction()
    {
        if (!playerInput || playerInput.actions == null) return;
        _scrollAction = playerInput.actions.FindAction(scrollActionName, throwIfNotFound: false);
    }

    void Update()
    {
        if (_scrollAction == null) return;

        Vector2 v = _scrollAction.ReadValue<Vector2>();
        float y = v.y;

        if (Mathf.Abs(y) < deadzone) return;
        if (invertY) y = -y;

        var sr = ResolveScrollRect();
        if (!sr) return;

        float next = sr.verticalNormalizedPosition + (y * speed * Time.unscaledDeltaTime);
        sr.verticalNormalizedPosition = Mathf.Clamp01(next);
    }

    ScrollRect ResolveScrollRect()
    {
        if (EventSystem.current)
        {
            var go = EventSystem.current.currentSelectedGameObject;
            if (go)
            {
                var sr = go.GetComponentInParent<ScrollRect>();
                if (sr) { _cached = sr; return sr; }
            }
        }

        if (_cached) return _cached;
        if (fallbackScrollRect) { _cached = fallbackScrollRect; return _cached; }
        return null;
    }
}