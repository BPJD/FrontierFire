using Michsky.UI.Heat;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class MainUI_KeyRebindItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private PanelButton keyMappingButton;
    [SerializeField] private TextMeshProUGUI buttonLabelOverride; // 선택: PanelButton 갱신 이슈 대비

    [Header("Binding Target")]
    [SerializeField] private string actionName;     // 예: "Jump"
    [SerializeField] private int bindingIndex = 0;  // 예: 0=Keyboard, 1=Gamepad (프로젝트 구성에 맞게)

    [Header("Options")]
    [SerializeField] private float pressThreshold = 0.5f;
    [SerializeField] private bool allowMouse = true;

    private bool waiting;
    private MainUI_KeyMapLoader loader;

    private void Awake()
    {
        loader = MainUI_KeyMapLoader.GetOrFind();
        if (loader == null)
        {
            Debug.LogError("[KeyRebindItem] KeyMapLoader not found.");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        // 메뉴 열릴 때마다 현재 바인딩을 버튼에 반영
        RefreshLabelFromCurrentBinding();
    }

    public void OnClick_Rebind()
    {
        if (!enabled) return;
        if (waiting) return;

        waiting = true;
        SetButtonText("...");

        InputSystem.onEvent -= OnInputEvent;
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!waiting) return;
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;

        foreach (var control in device.allControls)
        {
            // Keyboard anyKey 제외
            if (device is Keyboard && Keyboard.current != null && control == Keyboard.current.anyKey)
                continue;

            // 마우스 허용 여부
            if (!allowMouse && device is Mouse)
                continue;

            if (control is ButtonControl button)
            {
                if (button.ReadValueFromEvent(eventPtr, out float value) && value > pressThreshold)
                {
                    string display = ToDisplayName(control);
                    string path = control.path;

                    if (ApplyOverride(path))
                    {
                        SetButtonText(display);
                        loader.Save();
                    }
                    else
                    {
                        RefreshLabelFromCurrentBinding();
                    }

                    waiting = false;
                    InputSystem.onEvent -= OnInputEvent;
                    return;
                }
            }
        }
    }

    private bool ApplyOverride(string newPath)
    {
        var actions = loader.Actions;
        if (actions == null) return false;

        var act = actions.FindAction(actionName, throwIfNotFound: false);
        if (act == null)
        {
            Debug.LogError($"[KeyRebindItem] Action not found: {actionName}");
            return false;
        }

        if (bindingIndex < 0 || bindingIndex >= act.bindings.Count)
        {
            Debug.LogError($"[KeyRebindItem] Invalid bindingIndex={bindingIndex} for action={actionName}, count={act.bindings.Count}");
            return false;
        }

        // (중요) Composite 파트는 이 방식으로 직접 override하면 안 되는 경우가 많음.
        // 지금 리스트는 대부분 단일 버튼/키 액션이므로 OK.
        act.ApplyBindingOverride(bindingIndex, newPath);
        return true;
    }

    private void RefreshLabelFromCurrentBinding()
    {
        if (loader?.Actions == null) return;

        var act = loader.Actions.FindAction(actionName, throwIfNotFound: false);
        if (act == null) return;
        if (bindingIndex < 0 || bindingIndex >= act.bindings.Count) return;

        string effective = act.bindings[bindingIndex].effectivePath;
        if (string.IsNullOrEmpty(effective))
        {
            SetButtonText("None");
            return;
        }

        string human = InputControlPath.ToHumanReadableString(
            effective,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );

        // 트리거 표시 정규화(선택)
        human = human.Replace("Left Trigger", "LT").Replace("Right Trigger", "RT");

        SetButtonText(human);
    }

    private string ToDisplayName(InputControl control)
    {
        if (Gamepad.current != null)
        {
            if (control == Gamepad.current.leftTrigger) return "LT";
            if (control == Gamepad.current.rightTrigger) return "RT";
        }
        return control.displayName;
    }

    private void SetButtonText(string text)
    {
        if (keyMappingButton != null)
        {
            keyMappingButton.buttonText = text;
            keyMappingButton.UpdateUI(); // 너가 확인한 갱신 해결책
        }

        if (buttonLabelOverride != null)
            buttonLabelOverride.text = text;
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
        waiting = false;
    }
}
