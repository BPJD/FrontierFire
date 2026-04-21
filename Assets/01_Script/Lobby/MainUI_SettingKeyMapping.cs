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
    [SerializeField] private string targetGroup = "Keyboard&Mouse";

    [Header("Rebind")]
    [SerializeField] private float pressThreshold = 0.5f;
    [SerializeField] private bool allowMouse = true;

    [Header("Icon / Text")]
    [SerializeField] private DataKeyMapIcons keyMapIcons;
    [SerializeField] private Image keyIconImage;
    [SerializeField] private bool hideTextWhenIconFound = true;

    [SerializeField] private int bindingIndex = -1;

    private bool waiting;
    private MainUI_KeyMapLoader keyMapLoader;
    private UI_SoundPlayer uiSoundPlayer;

    public enum RebindTargetType
    {
        NormalButton,
        MoveGamepadStick,
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight
    }

    [SerializeField] private RebindTargetType rebindTargetType = RebindTargetType.NormalButton;

    private void Awake()
    {
        keyMapLoader = MainUI_KeyMapLoader.GetOrFind();
        if (keyMapLoader == null)
            enabled = false;
    }

    private void OnEnable()
    {
        ResolveBindingIndex();
        RefreshLabel();
        uiSoundPlayer = GetComponentInParent<UI_SoundPlayer>();
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
        waiting = false;
    }

    public void KeyMappingButtonClicked()
    {
        if (!enabled || waiting)
            return;

        ResolveBindingIndex();

        if (bindingIndex < 0)
            return;

        waiting = true;
        SetVisual("...", null);

        InputSystem.onEvent -= OnInputEvent;
        InputSystem.onEvent += OnInputEvent;

        if (uiSoundPlayer != null)
            uiSoundPlayer.PlayUIClickSound();
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!waiting) return;
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;

        if (IsCancel(eventPtr, device))
        {
            CancelRebind();
            return;
        }

        if (!allowMouse && device is Mouse)
            return;

        switch (rebindTargetType)
        {
            case RebindTargetType.MoveGamepadStick:
                TryRebindGamepadStick(eventPtr, device);
                break;

            case RebindTargetType.MoveUp:
            case RebindTargetType.MoveDown:
            case RebindTargetType.MoveLeft:
            case RebindTargetType.MoveRight:
                TryRebindKeyboardButton(eventPtr, device);
                break;

            default:
                TryRebindNormalButton(eventPtr, device);
                break;
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

    private void TryRebindNormalButton(InputEventPtr eventPtr, InputDevice device)
    {
        foreach (var control in device.allControls)
        {
            if (device is Keyboard keyboard && control == keyboard.anyKey)
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

            FinishRebind();
            return;
        }
    }

    private void TryRebindGamepadStick(InputEventPtr eventPtr, InputDevice device)
    {
        if (device is not Gamepad gamepad)
            return;

        var stick = gamepad.leftStick;
        if (stick == null)
            return;

        if (!stick.ReadValueFromEvent(eventPtr, out Vector2 value))
            return;

        if (value.magnitude < 0.5f)
            return;

        string newPath = "<Gamepad>/leftStick";

        if (ApplyBindingOverride(newPath))
            keyMapLoader.Save();

        FinishRebind();
    }

    private void TryRebindKeyboardButton(InputEventPtr eventPtr, InputDevice device)
    {
        if (device is not Keyboard keyboard)
            return;

        foreach (var control in device.allControls)
        {
            if (control == keyboard.anyKey)
                continue;

            if (control is not ButtonControl button)
                continue;

            if (!button.ReadValueFromEvent(eventPtr, out float value) || value <= pressThreshold)
                continue;

            string newPath = ToControlPath(device, control);

            if (ApplyBindingOverride(newPath))
                keyMapLoader.Save();

            FinishRebind();
            return;
        }
    }

    private void FinishRebind()
    {
        RefreshLabel();
        waiting = false;
        InputSystem.onEvent -= OnInputEvent;

        var loader = MainUI_KeyMapLoader.GetOrFind();
        var playerInput = FindFirstObjectByType<PlayerInput>();
        var inputController = FindFirstObjectByType<PlayerInputController>();

        if (loader != null && playerInput != null)
            loader.ApplyToPlayerInput(playerInput, true);

        if (inputController != null)
        {
            inputController.RefreshActionBindings();
            inputController.SetInputLock(false);
        }
    }


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

        if (uiSoundPlayer != null)
            uiSoundPlayer.PlayUIDenied();
    }

    public void ForceRefreshLabel()
    {
        RefreshLabel();
    }

    private void ResolveBindingIndex()
    {
        var actions = keyMapLoader?.Actions;
        if (actions == null)
        {
            bindingIndex = -1;
            return;
        }

        var act = actions.FindAction(actionName, throwIfNotFound: false);
        if (act == null)
        {
            bindingIndex = -1;
            return;
        }

        switch (rebindTargetType)
        {
            case RebindTargetType.NormalButton:
                bindingIndex = BindingIndexUtil.FindFirstBindingIndexByGroup(act, targetGroup);
                break;

            case RebindTargetType.MoveGamepadStick:
                bindingIndex = BindingIndexUtilEx.FindBindingIndexByExactPath(act, "Gamepad", "<Gamepad>/leftStick");
                break;

            case RebindTargetType.MoveUp:
                bindingIndex = BindingIndexUtilEx.FindCompositePartIndex(act, "Keyboard&Mouse", "up");
                break;

            case RebindTargetType.MoveDown:
                bindingIndex = BindingIndexUtilEx.FindCompositePartIndex(act, "Keyboard&Mouse", "down");
                break;

            case RebindTargetType.MoveLeft:
                bindingIndex = BindingIndexUtilEx.FindCompositePartIndex(act, "Keyboard&Mouse", "left");
                break;

            case RebindTargetType.MoveRight:
                bindingIndex = BindingIndexUtilEx.FindCompositePartIndex(act, "Keyboard&Mouse", "right");
                break;
        }
    }

    private bool ApplyBindingOverride(string newPath)
    {
        var actions = keyMapLoader?.Actions;
        if (actions == null)
            return false;

        var act = actions.FindAction(actionName, throwIfNotFound: false);
        if (act == null)
            return false;

        if (bindingIndex < 0 || bindingIndex >= act.bindings.Count)
            return false;

        act.ApplyBindingOverride(bindingIndex, newPath);

        if (uiSoundPlayer != null)
            uiSoundPlayer.PlayUIConfirm();

        return true;
    }

    private void RefreshLabel()
    {
        var actions = keyMapLoader?.Actions;
        if (actions == null)
            return;

        var act = actions.FindAction(actionName, throwIfNotFound: false);
        if (act == null)
            return;

        if (bindingIndex < 0)
            ResolveBindingIndex();

        if (bindingIndex < 0)
        {
            SetVisual("None", null);
            return;
        }

        string effective = act.bindings[bindingIndex].effectivePath;
        if (string.IsNullOrEmpty(effective))
        {
            SetVisual("None", null);
            return;
        }

        string normalized = NormalizePath(effective);
        string text = Abbrev(normalized);

        SetVisual(text, normalized);
    }

    private void SetVisual(string text, string normalizedPath)
    {
        keyMappingButton.buttonText = text;
        keyMappingButton.UpdateUI();

        if (keyIconImage != null)
        {
            keyIconImage.enabled = false;
            keyIconImage.sprite = null;
        }

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

    private static string ToControlPath(InputDevice device, InputControl control)
    {
        if (control == null)
            return string.Empty;

        string p = control.path;
        if (string.IsNullOrEmpty(p))
            return string.Empty;

        // 이미 완전한 경로면 그대로 사용
        if (p.StartsWith("<"))
            return p;

        string layout = device != null ? device.layout : control.device.layout;

        // "/Keyboard/w" 같은 케이스 처리
        if (p.StartsWith("/"))
        {
            string prefix = "/" + layout + "/";

            if (p.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                string controlPart = p.Substring(prefix.Length);
                return $"<{layout}>/{controlPart}";
            }

            return $"<{layout}>{p}";
        }

        // "w" 같은 순수 control 이름만 들어온 경우
        return $"<{layout}>/{p}";
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        if (path.StartsWith("<")) return path;

        if (path.StartsWith("/"))
            return $"<Mouse>{path}";

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

    private static string Abbrev(string normalizedPathOrKey)
    {
        if (string.IsNullOrEmpty(normalizedPathOrKey))
            return "None";

        string key = normalizedPathOrKey;
        int end = key.IndexOf(">/", System.StringComparison.Ordinal);
        if (end >= 0)
            key = key.Substring(end + 2);

        if (key.Contains("/") && !key.StartsWith("dpad/"))
            key = key.Split('/')[^1];

        if (key == "press") return "LMB";

        if (key == "leftButton") return "LMB";
        if (key == "rightButton") return "RMB";
        if (key == "middleButton") return "MMB";

        return key switch
        {
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
            "leftStick" => "L-Stick",
            "rightStick" => "R-Stick",
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

    public void PlayHoverSound()
    {
        if (uiSoundPlayer != null)
            uiSoundPlayer.PlayUIHoverSound();
    }
}