using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class UI_InputDeviceDetector : MonoBehaviour
{
    public enum InputType
    {
        None,
        KeyboardMouse,
        Gamepad
    }

    [Header("Axis Thresholds")]
    [Tooltip("마우스 이동이 이 값 이상이면 '입력'으로 인정")]
    [SerializeField] private float mouseDeltaThreshold = 0.5f;

    [Tooltip("스틱 이동(벡터 길이)이 이 값 이상이면 '입력'으로 인정")]
    [SerializeField] private float stickMagnitudeThreshold = 0.2f;

    [Tooltip("트리거 값이 이 값 이상이면 '입력'으로 인정")]
    [SerializeField] private float triggerThreshold = 0.2f;

    public InputType currentInputType { get; private set; } = InputType.None;

    /// <summary>
    /// 조작 기기 타입이 바뀌는 순간 호출됨.
    /// (newType, prevType, triggerDevice)
    /// </summary>
    public event Action<InputType, InputType, InputDevice> OnInputTypeChanged;

    UI_GamePadSelectController gamePadSelectController;

    private void OnEnable()
    {
        gamePadSelectController = GetComponent<UI_GamePadSelectController>();
        InputSystem.onEvent += OnInputEvent;

    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        if (device is Keyboard)
        {
            SwitchTo(InputType.KeyboardMouse, device, "키보드 조작이 감지되었습니다.");
            return;
        }

        if (device is Mouse mouse)
        {
            Vector2 delta = mouse.delta.ReadValue();
            float wheel = mouse.scroll.ReadValue().y;

            if (delta.sqrMagnitude >= mouseDeltaThreshold * mouseDeltaThreshold || Mathf.Abs(wheel) > 0.01f)
                SwitchTo(InputType.KeyboardMouse, device, "마우스 조작(이동/휠)이 감지되었습니다.");

            return;
        }

        if (device is Gamepad pad)
        {
            Vector2 left = pad.leftStick.ReadValue();
            Vector2 right = pad.rightStick.ReadValue();
            float lt = pad.leftTrigger.ReadValue();
            float rt = pad.rightTrigger.ReadValue();

            bool stickMoved =
                left.sqrMagnitude >= stickMagnitudeThreshold * stickMagnitudeThreshold ||
                right.sqrMagnitude >= stickMagnitudeThreshold * stickMagnitudeThreshold;

            bool triggerPulled = lt >= triggerThreshold || rt >= triggerThreshold;


            if (stickMoved || triggerPulled)
            {
                SwitchTo(InputType.Gamepad, device, "컨트롤러 조작(스틱/트리거)이 감지되었습니다.");
            }
            else
            {
                SwitchTo(InputType.Gamepad, device, "컨트롤러 조작이 감지되었습니다.");
            }

            return;
        }
    }


    private void SwitchTo(InputType nextType, InputDevice triggerDevice, string log)
    {
        if (currentInputType == nextType) return;

        var prev = currentInputType;
        currentInputType = nextType;

        //Debug.Log(log);

        if (nextType == InputType.KeyboardMouse)
        {
            gamePadSelectController.KeyboardDetected();
        }
        else
        {
            gamePadSelectController.GamePadDetected();
        }
        // 핵심: 타입 변경 이벤트 발행
        OnInputTypeChanged?.Invoke(nextType, prev, triggerDevice);
    }
}
