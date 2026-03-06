using UnityEngine;
using UnityEngine.InputSystem;

public class MainUI_UISwitcher : MonoBehaviour
{
    [SerializeField] private UI_InputDeviceDetector detector;

    [SerializeField] GameObject gamePadKey;
    [SerializeField] GameObject keyboardMouseKey;

    [SerializeField] GameObject[] gamePadUI;
    [SerializeField] GameObject[] keyboardUI;


    private void OnEnable()
    {
        detector.OnInputTypeChanged += HandleInputTypeChanged;
    }

    private void OnDisable()
    {
        detector.OnInputTypeChanged -= HandleInputTypeChanged;
    }

    private void HandleInputTypeChanged(
        UI_InputDeviceDetector.InputType newType,
        UI_InputDeviceDetector.InputType prevType,
        InputDevice triggerDevice)
    {
        // 여기서 원하는 메서드 호출
        if (newType == UI_InputDeviceDetector.InputType.Gamepad)
        {
            ShowUI(true);
        }
        else if (newType == UI_InputDeviceDetector.InputType.KeyboardMouse)
        {
            ShowUI(false);
        }
    }

    void ShowUI(bool isGamepad)
    {
        gamePadKey.SetActive(isGamepad);
        keyboardMouseKey.SetActive(!isGamepad);

        foreach (var gamePadUI in gamePadUI)
            gamePadUI.SetActive(isGamepad);

        foreach (var keyboardUI in keyboardUI)
            keyboardUI.SetActive(!isGamepad);

    }

}
