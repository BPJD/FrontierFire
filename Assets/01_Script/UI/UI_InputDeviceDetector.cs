using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

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

    private void OnEnable()
    {
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // 버튼/축/마우스 움직임 등 "상태 변화" 이벤트만 필터링
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        if (device is Keyboard)
        {
            SwitchTo(InputType.KeyboardMouse, "키보드 조작이 감지되었습니다.");
            return;
        }

        if (device is Mouse mouse)
        {
            // 마우스 이동(Delta) / 휠 등을 입력으로 인정
            // (StateEvent 기반이라 값 변화가 있으면 들어오지만, 임계치로 노이즈 컷)
            Vector2 delta = mouse.delta.ReadValue();
            float wheel = mouse.scroll.ReadValue().y;

            if (delta.sqrMagnitude >= mouseDeltaThreshold * mouseDeltaThreshold || Mathf.Abs(wheel) > 0.01f)
            {
                SwitchTo(InputType.KeyboardMouse, "마우스 조작(이동/휠)이 감지되었습니다.");
            }
            return;
        }

        if (device is Gamepad pad)
        {
            // 스틱/트리거/버튼 중 하나라도 유의미하면 Gamepad 입력으로 인정
            Vector2 left = pad.leftStick.ReadValue();
            Vector2 right = pad.rightStick.ReadValue();
            float lt = pad.leftTrigger.ReadValue();
            float rt = pad.rightTrigger.ReadValue();

            bool stickMoved =
                left.sqrMagnitude >= stickMagnitudeThreshold * stickMagnitudeThreshold ||
                right.sqrMagnitude >= stickMagnitudeThreshold * stickMagnitudeThreshold;

            bool triggerPulled =
                lt >= triggerThreshold || rt >= triggerThreshold;

            // 버튼은 별도 체크 없이도 이벤트가 오지만, 축만 쓰는 게임 대비로 조건을 명시
            if (stickMoved || triggerPulled)
            {
                SwitchTo(InputType.Gamepad, "컨트롤러 조작(스틱/트리거)이 감지되었습니다.");
            }
            else
            {
                // 버튼 입력도 감지하고 싶으면 아래 한 줄을 활성화하세요.
                // SwitchTo(InputType.Gamepad, "컨트롤러 조작(버튼)이 감지되었습니다.");
                // 단, 버튼은 이벤트 난사 가능하니 currentInputType 변경시에만 로그가 나가게 되어있습니다.
                SwitchTo(InputType.Gamepad, "컨트롤러 조작이 감지되었습니다.");
            }

            return;
        }

        // 기타 장치(터치스크린, 조이스틱 등) 확장 지점
        // 필요 시 device 타입별로 분기 추가
    }

    private void SwitchTo(InputType type, string log)
    {
        if (currentInputType == type) return;
        currentInputType = type;
        Debug.Log(log);
    }
}
