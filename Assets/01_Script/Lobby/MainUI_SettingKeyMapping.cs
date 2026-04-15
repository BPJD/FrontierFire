using Michsky.UI.Heat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class MainUI_SettingKeyMapping : MonoBehaviour
{
    [SerializeField] private PanelButton keyMappingButton;

    [Header("Target")]
    [SerializeField] private string actionName = "Jump";
    [SerializeField] private string targetGroup = "Keyboard&Mouse"; // 또는 "Gamepad"

    [Header("Rebind")]
    [SerializeField] private float pressThreshold = 0.5f;
    [SerializeField] private bool allowMouse = true;

    [Header("Icon / Text")]
    [SerializeField] private DataKeyMapIcons keyMapIcons; // Gamepad/Mouse 아이콘 DB(없으면 텍스트만)
    [SerializeField] private Image keyIconImage;
    [SerializeField] private bool hideTextWhenIconFound = true;

    // Debug/inspect
    [SerializeField] private int bindingIndex = -1;

    private bool waiting;
    private MainUI_KeyMapLoader keyMapLoader;



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
        RefreshLabel();
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
        waiting = false;
    }

    public void KeyMappingButtonClicked()
    {
        if (!enabled || waiting) return;

        ResolveBindingIndex();
        if (bindingIndex < 0)
        {
            Debug.LogError($"[KeyMap] Cannot rebind. action={actionName}, group={targetGroup} binding not found.");
            return;
        }

        waiting = true;
        SetVisual("...", null);

        InputSystem.onEvent -= OnInputEvent;
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!waiting) return;
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;

        // Cancel: ESC / Gamepad Start
        if (IsCancel(eventPtr, device))
        {
            CancelRebind();
            return;
        }

        if (!allowMouse && device is Mouse)
            return;

        foreach (var control in device.allControls)
        {
            if (device is Keyboard && Keyboard.current != null && control == Keyboard.current.anyKey)
                continue;

            if (control is not ButtonControl button)
                continue;

            if (IsDisallowedControl(control))
                continue;

            if (!button.ReadValueFromEvent(eventPtr, out float value) || value <= pressThreshold)
                continue;

            string newPath = ToControlPath(device, control);

            if (ApplyBindingOverride(newPath))
                keyMapLoader.Save();

            RefreshLabel();

            waiting = false;
            InputSystem.onEvent -= OnInputEvent;
            return;
        }
    }

    private bool IsCancel(InputEventPtr eventPtr, InputDevice device)
    {
        if (device is Keyboard && Keyboard.current != null)
        {
            var esc = Keyboard.current.escapeKey;
            if (esc != null && esc.ReadValueFromEvent(eventPtr, out float v) && v > pressThreshold)
                return true;
        }

        if (device is Gamepad && Gamepad.current != null)
        {
            var start = Gamepad.current.startButton;
            if (start != null && start.ReadValueFromEvent(eventPtr, out float v) && v > pressThreshold)
                return true;
        }

        return false;
    }

    // Gamepad 스틱 "기울임(방향 버튼)" 제외. L3/R3(press)는 허용.
    private bool IsDisallowedControl(InputControl control)
    {
        if (control?.device is not Gamepad)
            return false;

        if (control.name == "leftStickPress" || control.name == "rightStickPress")
            return false;

        string p = control.path;
        return !string.IsNullOrEmpty(p) && (p.Contains("leftStick") || p.Contains("rightStick"));
    }

    private void CancelRebind()
    {
        waiting = false;
        InputSystem.onEvent -= OnInputEvent;
        RefreshLabel();
    }

    public void ForceRefreshLabel() => RefreshLabel();

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

    private void RefreshLabel()
    {
        var actions = keyMapLoader?.Actions;
        if (actions == null) return;

        var act = actions.FindAction(actionName, throwIfNotFound: false);
        if (act == null) return;

        if (bindingIndex < 0) ResolveBindingIndex();
        if (bindingIndex < 0) { SetVisual("None", null); return; }

        string effective = act.bindings[bindingIndex].effectivePath;
        if (string.IsNullOrEmpty(effective)) { SetVisual("None", null); return; }

        string normalized = NormalizePath(effective);

        // 항상 안정적으로 텍스트 축약
        string text = Abbrev(normalized);

        SetVisual(text, normalized);
    }

    private void SetVisual(string text, string normalizedPath)
    {
        // 1) 텍스트
        keyMappingButton.buttonText = text;
        keyMappingButton.UpdateUI();

        // 2) 아이콘 초기화
        if (keyIconImage != null)
        {
            keyIconImage.enabled = false;
            keyIconImage.sprite = null;
        }

        // 3) 아이콘(있으면 표시)
        if (keyMapIcons == null || keyIconImage == null) return;
        if (string.IsNullOrEmpty(normalizedPath)) return;

        Sprite icon = keyMapIcons.GetGamepadIcon(normalizedPath);
        if (icon == null) return;

        keyIconImage.sprite = icon;
        keyIconImage.enabled = true;

        if (hideTextWhenIconFound)
        {
            keyMappingButton.buttonText = "";
            keyMappingButton.UpdateUI();
        }
    }

    // -------------------------
    // Path utils
    // -------------------------
    private static string ToControlPath(InputDevice device, InputControl control)
    {
        // control.path는 보통 "/rightShoulder" 같이 오므로 "<Layout>/..."로 보정
        string p = control.path;
        if (p.StartsWith("<")) return p;

        if (p.StartsWith("/"))
            return $"<{device.layout}>{p}";

        return $"<{device.layout}>/{p}";
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        if (path.StartsWith("<")) return path;

        // "/leftButton" 같은 케이스 → Mouse 기본
        if (path.StartsWith("/"))
            return $"<Mouse>{path}";

        // "Keyboard/a", "Mouse/press", "XInputControllerWindows1/rightShoulder" → "<Keyboard>/a" 형태로
        int slash = path.IndexOf('/');
        if (slash > 0)
        {
            string deviceName = StripTrailingDigits(path.Substring(0, slash));
            string control = path.Substring(slash + 1);
            return $"<{deviceName}>/{control}";
        }

        return path;
    }

    private static string StripTrailingDigits(string s)
    {
        int i = s.Length - 1;
        while (i >= 0 && char.IsDigit(s[i])) i--;
        return s.Substring(0, i + 1);
    }

    // -------------------------
    // Text abbrev
    // -------------------------
    private static string Abbrev(string normalizedPathOrKey)
    {
        if (string.IsNullOrEmpty(normalizedPathOrKey))
            return "None";

        // "<Device>/x" -> "x"
        string key = normalizedPathOrKey;
        int end = key.IndexOf(">/", System.StringComparison.Ordinal);
        if (end >= 0)
            key = key.Substring(end + 2);

        // dpad/up 유지, 그 외는 마지막 토큰만
        if (key.Contains("/") && !key.StartsWith("dpad/"))
            key = key.Split('/')[^1];

        // Pointer press(또는 Mouse/press 류) -> LMB
        if (key == "press") return "LMB";

        // Mouse
        if (key == "leftButton") return "LMB";
        if (key == "rightButton") return "RMB";
        if (key == "middleButton") return "MMB";

        // Gamepad / Keyboard common
        return key switch
        {
            // Gamepad
            "buttonSouth" => "A",
            "buttonEast" => "B",
            "buttonWest" => "X",
            "buttonNorth" => "Y",
            "leftShoulder" => "LB",
            "rightShoulder" => "RB",
            "leftTrigger" => "LT",
            "rightTrigger" => "RT",
            "leftStickPress" => "L3",
            "rightStickPress" => "R3",
            "startButton" => "Start",
            "selectButton" or "menuButton" => "Select",
            "dpad/up" => "D↑",
            "dpad/down" => "D↓",
            "dpad/left" => "D←",
            "dpad/right" => "D→",

            // Keyboard
            "escape" => "Esc",
            "space" => "Space",
            "enter" => "Enter",
            "tab" => "Tab",
            "backspace" => "Bksp",
            "delete" => "Del",
            "leftShift" => "LShift",
            "rightShift" => "RShift",
            "leftCtrl" => "Ctrl",
            "rightCtrl" => "RCtrl",
            "leftAlt" => "Alt",
            "rightAlt" => "RAlt",
            "upArrow" => "↑",
            "downArrow" => "↓",
            "leftArrow" => "←",
            "rightArrow" => "→",

            _ => key.Length == 1 ? key.ToUpperInvariant() : key
        };
    }
}
