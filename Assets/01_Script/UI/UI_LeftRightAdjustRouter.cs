using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_LeftRightAdjustRouter : MonoBehaviour
{
    [Header("Input Source")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string navigateActionName = "Navigate"; // UI/Navigate

    [Header("Behavior")]
    [SerializeField] private float deadzone = 0.6f;           // 좌/우 판정(스틱/패드)
    [SerializeField] private float repeatDelay = 0.30f;       // 처음 누른 뒤 반복 시작
    [SerializeField] private float repeatRate = 0.10f;        // 반복 속도
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Priority")]
    [Tooltip("선택된 오브젝트가 Adapter 영역 안에 있으면, Slider보다 Adapter를 우선 처리")]
    [SerializeField] private bool preferAdapterOverSlider = true;

    [Header("Slider")]
    [Tooltip("float 슬라이더(wholeNumbers=false)일 때 normalized 기준 이동량")]
    [SerializeField] private float sliderStepNormalized = 0.05f;

    private InputAction _nav;
    private float _nextTime;
    private bool _held;
    private int _heldDir;

    void Awake()
    {
        if (!playerInput)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (playerInput?.actions != null)
            _nav = playerInput.actions.FindAction(navigateActionName, throwIfNotFound: false);

        if (_nav == null)
            Debug.LogWarning($"[UI_LeftRightAdjustRouter] Navigate action not found: '{navigateActionName}'");
    }

    void OnEnable() => _nav?.Enable();
    void OnDisable() => _nav?.Disable();

    void Update()
    {
        if (_nav == null || EventSystem.current == null) return;

        float now = useUnscaledTime ? Time.unscaledTime : Time.time;

        Vector2 v = _nav.ReadValue<Vector2>();
        int dir = 0;
        if (v.x <= -deadzone) dir = -1;
        else if (v.x >= deadzone) dir = +1;

        if (dir == 0)
        {
            _held = false;
            return;
        }

        var go = EventSystem.current.currentSelectedGameObject;
        if (!go) return;

        // 조정 대상이 아니면 기본 네비게이션에 맡김
        if (!IsAdjustable(go)) return;

        // 첫 입력은 즉시 1회 실행 + 홀드 반복
        if (!_held || dir != _heldDir)
        {
            Apply(go, dir);
            _held = true;
            _heldDir = dir;
            _nextTime = now + repeatDelay;
            return;
        }

        if (now >= _nextTime)
        {
            Apply(go, dir);
            _nextTime = now + repeatRate;
        }
    }

    bool IsAdjustable(GameObject selected)
    {
        // Adapter 또는 Slider 중 하나라도 잡히면 조정 대상으로 취급
        if (preferAdapterOverSlider)
        {
            var adapter = selected.GetComponentInParent<ILeftRightAdjustable>();
            if (adapter != null && adapter.IsInteractable) return true;

            var slider = selected.GetComponentInParent<Slider>();
            if (slider != null && slider.interactable) return true;
        }
        else
        {
            var slider = selected.GetComponentInParent<Slider>();
            if (slider != null && slider.interactable) return true;

            var adapter = selected.GetComponentInParent<ILeftRightAdjustable>();
            if (adapter != null && adapter.IsInteractable) return true;
        }

        return false;
    }

    void Apply(GameObject selected, int dir)
    {
        if (preferAdapterOverSlider)
        {
            if (TryAdapter(selected, dir)) return;
            if (TrySlider(selected, dir)) return;
        }
        else
        {
            if (TrySlider(selected, dir)) return;
            if (TryAdapter(selected, dir)) return;
        }
    }

    bool TryAdapter(GameObject selected, int dir)
    {
        var adapter = selected.GetComponentInParent<ILeftRightAdjustable>();
        if (adapter == null || !adapter.IsInteractable) return false;

        if (dir < 0) adapter.Prev();
        else adapter.Next();
        return true;
    }

    bool TrySlider(GameObject selected, int dir)
    {
        var slider = selected.GetComponentInParent<Slider>();
        if (slider == null || !slider.interactable) return false;

        ApplyToSlider(slider, dir);
        return true;
    }

    void ApplyToSlider(Slider slider, int dir)
    {
        if (slider.wholeNumbers)
        {
            // 정수 슬라이더는 1 스텝이 기본(원하면 별도 step 변수 추가 가능)
            slider.value = Mathf.Clamp(slider.value + dir * 1f, slider.minValue, slider.maxValue);
            return;
        }

        // float 슬라이더는 normalizedValue 기준으로 일정 비율 이동
        float n = slider.normalizedValue;
        n = Mathf.Clamp01(n + dir * sliderStepNormalized);
        slider.normalizedValue = n;
    }
}

// ---- 확장용 인터페이스 ----
public interface ILeftRightAdjustable
{
    bool IsInteractable { get; }
    void Prev();
    void Next();
}