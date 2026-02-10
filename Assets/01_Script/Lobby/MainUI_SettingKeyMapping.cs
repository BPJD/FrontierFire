using Michsky.UI.Heat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class MainUI_SettingKeyMapping : MonoBehaviour
{
    [SerializeField] PanelButton keyMappingButton;

    [Header("Target Action")]
    [SerializeField] string actionName = "Jump";

    [Header("Target Binding Group")]
    [SerializeField] string targetGroup = "Keyboard&Mouse"; // 또는 "Gamepad"

    // 자동 계산된 결과(디버그 확인용으로 보관)
    [SerializeField] int bindingIndex = -1;

    [Header("Rebind Options")]
    [SerializeField] float pressThreshold = 0.5f;
    [SerializeField] bool allowMouse = true;

    bool waiting;
    MainUI_KeyMapLoader keyMapLoader;

    private void Awake()
    {
        keyMapLoader = MainUI_KeyMapLoader.GetOrFind();
        if (keyMapLoader == null)
        {
            Debug.LogError("[KeyMap] KeyMapLoader not found.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        ResolveBindingIndex();
        RefreshLabelFromCurrentBinding();
    }

    public void KeyMappingButtonClicked()
    {
        if (!enabled) return;
        if (waiting) return;

        ResolveBindingIndex();
        if (bindingIndex < 0)
        {
            Debug.LogError($"[KeyMap] Cannot rebind. action={actionName}, group={targetGroup} binding not found.");
            return;
        }

        waiting = true;
        SetButtonText("...");

        InputSystem.onEvent -= OnInputEvent;
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!waiting) return;
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;

        if (!allowMouse && device is Mouse)
            return;

        foreach (var control in device.allControls)
        {
            if (device is Keyboard && Keyboard.current != null && control == Keyboard.current.anyKey)
                continue;

            if (control is ButtonControl button)
            {
                if (button.ReadValueFromEvent(eventPtr, out float value) && value > pressThreshold)
                {
                    string display = NormalizeDisplay(control);
                    string path = control.path;

                    if (ApplyBindingOverride(path))
                    {
                        SetButtonText(display);
                        keyMapLoader.Save();
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

    private void ResolveBindingIndex()
    {
        var actions = keyMapLoader?.Actions;
        if (actions == null) { bindingIndex = -1; return; }

        var act = actions.FindAction(actionName, throwIfNotFound: false);
        if (act == null) { bindingIndex = -1; return; }

        bindingIndex = BindingIndexUtil.FindFirstBindingIndexByGroup(act, targetGroup);
    }

    private bool ApplyBindingOverride(string newPath)
    {
        var actions = keyMapLoader.Actions;
        var act = actions.FindAction(actionName, throwIfNotFound: false);
        if (act == null) return false;
        if (bindingIndex < 0 || bindingIndex >= act.bindings.Count) return false;

        act.ApplyBindingOverride(bindingIndex, newPath);
        return true;
    }

    private void RefreshLabelFromCurrentBinding()
    {
        var actions = keyMapLoader?.Actions;
        if (actions == null) return;

        var act = actions.FindAction(actionName, throwIfNotFound: false);
        if (act == null) return;

        if (bindingIndex < 0) ResolveBindingIndex();
        if (bindingIndex < 0) { SetButtonText("None"); return; }

        string effective = act.bindings[bindingIndex].effectivePath;
        if (string.IsNullOrEmpty(effective)) { SetButtonText("None"); return; }

        string human = InputControlPath.ToHumanReadableString(
            effective,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );

        human = human.Replace("Left Trigger", "LT").Replace("Right Trigger", "RT");
        SetButtonText(human);
    }

    private string NormalizeDisplay(InputControl control)
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
        keyMappingButton.buttonText = text;
        keyMappingButton.UpdateUI();
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
        waiting = false;
    }

    public void ForceRefreshLabel()
    {
        // 기존에 만들었던 RefreshLabelFromCurrentBinding()을 public 래핑
        RefreshLabelFromCurrentBinding();
    }


}
