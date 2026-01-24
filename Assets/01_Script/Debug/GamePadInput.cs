// HeldInputLogger.cs
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GamePadInput : MonoBehaviour
{
    [Header("General")]
    [SerializeField] float pollInterval = 0.15f;     // 폴링 간격
    [SerializeField] bool logOnChangeOnly = true;    // 변화가 있을 때만 로그
    [SerializeField] bool includeDeviceLevel = true; // 디바이스 레벨 로깅
    [SerializeField] bool includeActionLevel = false;// 액션 레벨 로깅

    [Header("Device-Level Options")]
    [SerializeField] bool includeKeyboard = true;
    [SerializeField] bool includeMouse = true;
    [SerializeField] bool includeGamepad = true;
    [SerializeField] bool logAxes = true;            // 스틱/마우스 이동도 표시
    [SerializeField] float axisThreshold = 0.4f;     // 스틱/축 임계치

    [Header("Action-Level Options")]
    [SerializeField] PlayerInput playerInput;        // 비워두면 Player 태그에서 자동 찾기
    [SerializeField] float valueMagnitudeThreshold = 0.4f; // Value 액션 임계치

    HashSet<string> lastDeviceDown = new HashSet<string>();
    HashSet<string> lastActionDown = new HashSet<string>();
    Coroutine pollCo;

    [SerializeField] TMPro.TextMeshProUGUI txt;

    void OnEnable()
    {
        if (playerInput == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) playerInput = go.GetComponent<PlayerInput>();
        }
        pollCo = StartCoroutine(PollLoop());
    }

    void OnDisable()
    {
        if (pollCo != null) StopCoroutine(pollCo);
        pollCo = null;
        lastDeviceDown.Clear();
        lastActionDown.Clear();
    }

    IEnumerator PollLoop()
    {
        var wait = new WaitForSeconds(pollInterval);
        while (true)
        {
            if (includeDeviceLevel) PollDevices();
            if (includeActionLevel && playerInput != null) PollActions();
            yield return wait;
        }
    }

    // ---------- Device Level ----------
    void PollDevices()
    {
        var now = new HashSet<string>();

        if (includeKeyboard && Keyboard.current != null)
            CollectKeyboard(now, Keyboard.current);

        if (includeMouse && Mouse.current != null)
            CollectMouse(now, Mouse.current);

        if (includeGamepad && Gamepad.current != null)
            CollectGamepad(now, Gamepad.current);

        LogDiff("[Held-Device]", lastDeviceDown, now);
        lastDeviceDown = now;

    }

    void CollectKeyboard(HashSet<string> set, Keyboard kb)
    {
        foreach (var key in kb.allKeys)
            if (key.isPressed)
                set.Add(Human(key));
    }

    void CollectMouse(HashSet<string> set, Mouse m)
    {
        if (m.leftButton.isPressed) set.Add(Human(m.leftButton));
        if (m.rightButton.isPressed) set.Add(Human(m.rightButton));
        if (m.middleButton.isPressed) set.Add(Human(m.middleButton));
        if (logAxes)
        {
            var delta = m.delta.ReadValue();
            if (delta.sqrMagnitude > axisThreshold * axisThreshold)
                set.Add($"Mouse Δ{delta}");
            if (m.scroll.ReadValue().y != 0)
                set.Add($"Scroll {m.scroll.ReadValue().y}");
        }
    }

    void CollectGamepad(HashSet<string> set, Gamepad g)
    {
        // Buttons
        AddIfPressed(set, g.buttonSouth);
        AddIfPressed(set, g.buttonEast);
        AddIfPressed(set, g.buttonWest);
        AddIfPressed(set, g.buttonNorth);
        AddIfPressed(set, g.leftShoulder);
        AddIfPressed(set, g.rightShoulder);
        AddIfPressed(set, g.leftStickButton);
        AddIfPressed(set, g.rightStickButton);
        AddIfPressed(set, g.startButton);
        AddIfPressed(set, g.selectButton);
        AddIfPressed(set, g.dpad.up);
        AddIfPressed(set, g.dpad.down);
        AddIfPressed(set, g.dpad.left);
        AddIfPressed(set, g.dpad.right);
        // Triggers (ButtonControl로도 동작하지만 값도 함께 보고 싶다면)
        if (g.leftTrigger.ReadValue() > 0.5f) set.Add($"LT {g.leftTrigger.ReadValue():0.00}");
        if (g.rightTrigger.ReadValue() > 0.5f) set.Add($"RT {g.rightTrigger.ReadValue():0.00}");

        if (logAxes)
        {
            var ls = g.leftStick.ReadValue();
            var rs = g.rightStick.ReadValue();
            if (ls.sqrMagnitude > axisThreshold * axisThreshold) set.Add($"LS {ls}");
            if (rs.sqrMagnitude > axisThreshold * axisThreshold) set.Add($"RS {rs}");
        }
    }

    void AddIfPressed(HashSet<string> set, ButtonControl btn)
    {
        if (btn != null && btn.isPressed)
            set.Add(Human(btn));
    }

    string Human(InputControl ctrl)
    {
        var nice = InputControlPath.ToHumanReadableString(
            ctrl.path,
            InputControlPath.HumanReadableStringOptions.OmitDevice |
            InputControlPath.HumanReadableStringOptions.UseShortNames
        );
        return $"{nice} ({ctrl.device.displayName})";
    }

    // ---------- Action Level ----------
    void PollActions()
    {
        var now = new HashSet<string>();
        if (playerInput == null) return;

        foreach (var act in playerInput.actions)
        {
            // Button 액션: IsPressed로 홀드 판단
            if (act.type == InputActionType.Button)
            {
                if (act.IsPressed())
                {
                    var ctrl = act.activeControl;
                    string label = ctrl != null
                        ? $"{act.name} by {InputControlPath.ToHumanReadableString(ctrl.path, InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames)}"
                        : $"{act.name}";
                    now.Add(label);
                }
            }
            else // Value/PassThrough: 값 크기로 홀드성 판단
            {
                // Vector2/float 등 범용 처리
                object obj = act.ReadValueAsObject();
                if (obj is float f)
                {
                    if (Mathf.Abs(f) > valueMagnitudeThreshold)
                        now.Add($"{act.name}={f:0.00}");
                }
                else if (obj is Vector2 v2)
                {
                    if (v2.magnitude > valueMagnitudeThreshold)
                        now.Add($"{act.name}={v2}");
                }
                else if (obj is Vector3 v3)
                {
                    if (v3.magnitude > valueMagnitudeThreshold)
                        now.Add($"{act.name}={v3}");
                }
                // 필요하면 다른 타입도 추가
            }
        }

        LogDiff("[Held-Action]", lastActionDown, now);
        lastActionDown = now;
    }

    // ---------- Diff Logger ----------
    void LogDiff(string prefix, HashSet<string> last, HashSet<string> now)
    {
        if (!logOnChangeOnly)
        {
            if (now.Count == 0) Debug.Log($"{prefix} (none)");
            else Debug.Log($"{prefix} " + string.Join(", ", now));
            txt.text = $"{prefix} " + string.Join(", ", now);
            return;
        }


        // 변화만 출력
        foreach (var added in Diff(now, last))
            Debug.Log($"{prefix} + {added}");

        foreach (var removed in Diff(last, now))
            Debug.Log($"{prefix} - {removed}");
    }

    IEnumerable<string> Diff(HashSet<string> a, HashSet<string> b)
    {
        foreach (var x in a)
            if (!b.Contains(x)) yield return x;
    }
}
