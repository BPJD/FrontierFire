using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_SettingTabControl : MonoBehaviour
{
    [Header("Input Source")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Action Names (UI ActionMap)")]
    [SerializeField] private string tabLeftActionName = "LeftMenu";   // UI/LeftMenu (LB)
    [SerializeField] private string tabRightActionName = "RightMenu"; // UI/RightMenu (RB)

    [Header("Tabs (order matters)")]
    [SerializeField] private Selectable[] tabs; // 탭 버튼들 (좌->우 순서)

    [Header("Behavior")]
    [SerializeField] private bool invokeOnSwitch = true;        // 탭이 클릭 기반이면 true
    [SerializeField] private bool wrapAround = true;            // 마지막->처음 순환
    [SerializeField] private float inputCooldown = 0.12f;       // RB/LB 연타/중복 프레임 방지(권장 0.1~0.2)
    [SerializeField] private bool forceSelectNextFrame = true;  // Heat UI가 선택 덮어쓰는 것 방지
    [SerializeField] private bool enableActionsOnEnable = true; // 이 스크립트가 액션 Enable/Disable을 직접 할지

    [Header("State (Debug)")]
    [SerializeField] private int cur = 0;

    private InputAction _tabLeftAction;
    private InputAction _tabRightAction;

    private float _nextAllowedTime;
    private Coroutine _forceSelectCo;

    void Awake()
    {
        if (!playerInput)
            playerInput = GetComponentInParent<PlayerInput>() ?? FindFirstObjectByType<PlayerInput>();

        CacheActions();
    }

    void OnEnable()
    {
        CacheActions();
        Bind(true);

        // 패널이 켜질 때 초기 탭 선택을 한번 고정(선택사항)
        if (forceSelectNextFrame)
            ForceSelect(cur, invoke: false);
        else
            SelectNow(cur, invoke: false);
    }

    void OnDisable()
    {
        Bind(false);

        if (_forceSelectCo != null)
        {
            StopCoroutine(_forceSelectCo);
            _forceSelectCo = null;
        }
    }

    void CacheActions()
    {
        if (!playerInput || playerInput.actions == null)
            return;

        _tabLeftAction = playerInput.actions.FindAction(tabLeftActionName, throwIfNotFound: false);
        _tabRightAction = playerInput.actions.FindAction(tabRightActionName, throwIfNotFound: false);

        if (_tabLeftAction == null)
            Debug.LogWarning($"[UI_SettingTabControl] Action not found: '{tabLeftActionName}'. Check InputActions(UI map).");
        if (_tabRightAction == null)
            Debug.LogWarning($"[UI_SettingTabControl] Action not found: '{tabRightActionName}'. Check InputActions(UI map).");
    }

    void Bind(bool bind)
    {
        if (_tabLeftAction != null)
        {
            if (enableActionsOnEnable)
            {
                if (bind) _tabLeftAction.Enable();
                else _tabLeftAction.Disable();
            }

            if (bind) _tabLeftAction.performed += OnTabLeft;
            else _tabLeftAction.performed -= OnTabLeft;
        }

        if (_tabRightAction != null)
        {
            if (enableActionsOnEnable)
            {
                if (bind) _tabRightAction.Enable();
                else _tabRightAction.Disable();
            }

            if (bind) _tabRightAction.performed += OnTabRight;
            else _tabRightAction.performed -= OnTabRight;
        }
    }

    void OnTabLeft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Switch(-1);
    }

    void OnTabRight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Switch(+1);
    }

    public void SetCurrentTab(int index, bool invoke = true)
    {
        if (tabs == null || tabs.Length == 0) return;

        index = Mathf.Clamp(index, 0, tabs.Length - 1);
        cur = index;

        if (forceSelectNextFrame) ForceSelect(cur, invoke);
        else SelectNow(cur, invoke);
    }

    void Switch(int dir)
    {
        if (tabs == null || tabs.Length == 0) return;

        if (Time.unscaledTime < _nextAllowedTime) return;
        _nextAllowedTime = Time.unscaledTime + inputCooldown;

        int next = ComputeNextIndex(cur, dir);
        if (next == cur) return;

        cur = next;

        if (forceSelectNextFrame) ForceSelect(cur, invokeOnSwitch);
        else SelectNow(cur, invokeOnSwitch);
    }

    int ComputeNextIndex(int current, int dir)
    {
        int len = tabs.Length;
        if (len <= 0) return 0;

        int next = current + dir;

        if (wrapAround)
            next = (next % len + len) % len; // 음수 안전 mod
        else
            next = Mathf.Clamp(next, 0, len - 1);

        // 비활성/인터랙트 불가 탭 스킵(최대 len번 시도)
        for (int i = 0; i < len; i++)
        {
            var t = tabs[next];
            if (t && t.gameObject.activeInHierarchy && t.IsInteractable())
                return next;

            next = wrapAround
                ? ((next + dir) % len + len) % len
                : Mathf.Clamp(next + dir, 0, len - 1);
        }

        // 전부 불가면 그대로
        return current;
    }

    void SelectNow(int index, bool invoke)
    {
        var target = GetSelectable(index);
        if (!target) return;

        if (EventSystem.current)
            EventSystem.current.SetSelectedGameObject(target.gameObject);

        if (invoke)
            InvokeSelectable(target);
    }

    void ForceSelect(int index, bool invoke)
    {
        if (_forceSelectCo != null) StopCoroutine(_forceSelectCo);
        _forceSelectCo = StartCoroutine(CoForceSelect(index, invoke));
    }

    IEnumerator CoForceSelect(int index, bool invoke)
    {
        // Heat UI/PanelManager가 이번 프레임에 선택을 만질 수 있어 1프레임 뒤 고정
        yield return null;

        var target = GetSelectable(index);
        if (!target) yield break;

        if (EventSystem.current)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }

        if (invoke)
            InvokeSelectable(target);

        _forceSelectCo = null;
    }

    Selectable GetSelectable(int index)
    {
        if (tabs == null || tabs.Length == 0) return null;
        if (index < 0 || index >= tabs.Length) return null;

        var t = tabs[index];
        if (!t) return null;
        if (!t.gameObject.activeInHierarchy) return null;

        return t;
    }

    void InvokeSelectable(Selectable target)
    {
        // Unity Button이면 클릭 호출
        var btn = target.GetComponent<Button>();
        if (btn)
        {
            btn.onClick.Invoke();
            return;
        }

        // Heat UI 전용 버튼이라면 여기서 해당 컴포넌트 메서드를 호출하도록 확장
        // 예)
        // var panelBtn = target.GetComponent<PanelButton>();
        // if (panelBtn) panelBtn.SetSelected(); // 실제 API에 맞게 수정
    }
}