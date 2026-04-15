using UnityEngine;
using UnityEngine.EventSystems;

public class DataKeyMapIcons : MonoBehaviour
{
    [SerializeField] Sprite gamepad_A;
    [SerializeField] Sprite gamepad_B;
    [SerializeField] Sprite gamepad_Y;
    [SerializeField] Sprite gamepad_X;
    [SerializeField] Sprite gamepad_DpadUp;
    [SerializeField] Sprite gamepad_DpadDown;
    [SerializeField] Sprite gamepad_DpadLeft;
    [SerializeField] Sprite gamepad_DpadRight;
    [SerializeField] Sprite gamepad_LeftBumper;
    [SerializeField] Sprite gamepad_RightBumper;
    [SerializeField] Sprite gamepad_LeftTrigger;
    [SerializeField] Sprite gamepad_RightTrigger;
    [SerializeField] Sprite gamepad_LeftStick;
    [SerializeField] Sprite gamepad_RightStick;
    [SerializeField] Sprite gamepad_Start;
    [SerializeField] Sprite gamepad_Select;
    [SerializeField] Sprite gamepad_L3;
    [SerializeField] Sprite gamepad_R3;

    [SerializeField] Sprite mouseLclick;
    [SerializeField] Sprite mouseRclick;
    [SerializeField] Sprite mouseMclick;

    UI_InputDeviceDetector inputDetector;
    [SerializeField] GameObject firstSelect;
    //[SerializeField] Sprite MouseLF;
    //[SerializeField] Sprite MouseLR;

    [Header("추가 아이콘")]
    [SerializeField] Sprite icon_Keyboard;

    private void OnEnable()
    {
        if (inputDetector == null)
        {
            inputDetector = GameObject.FindGameObjectWithTag("Module").GetComponent<UI_InputDeviceDetector>();
        }

        switch (inputDetector.currentInputType)
        {
            case UI_InputDeviceDetector.InputType.Gamepad:
                if(firstSelect != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstSelect);
                }
                break;
        }
    }

    public Sprite GetGamepadIcon(string controlPathOrName)
    {
        if (string.IsNullOrEmpty(controlPathOrName))
            return null;

        string key = controlPathOrName;

        // 1) "<...>/xxx" 형태면 디바이스 레이아웃 프리픽스 제거
        int end = key.IndexOf(">/", System.StringComparison.Ordinal);
        if (end >= 0)
            key = key.Substring(end + 2);

        // 2) "XInputControllerWindows1/rightShoulder" 형태면 앞 프리픽스 제거
        //    (dpad/up 같은 건 그대로 둬야 해서 조건을 둠)
        if (key.Contains("/") && !key.StartsWith("dpad/"))
        {
            var parts = key.Split('/');
            if (parts.Length == 2)
                key = parts[1];
        }

        // 3) dpad는 경로 기반 매칭
        if (key.Contains("dpad/up")) return gamepad_DpadUp;
        if (key.Contains("dpad/down")) return gamepad_DpadDown;
        if (key.Contains("dpad/left")) return gamepad_DpadLeft;
        if (key.Contains("dpad/right")) return gamepad_DpadRight;

        if (key == "leftStick" || key.StartsWith("leftStick/")) return gamepad_LeftStick;
        if (key == "rightStick" || key.StartsWith("rightStick/")) return gamepad_RightStick;

        return key switch
        {
            // ------------------
            // Gamepad
            // ------------------
            "buttonSouth" => gamepad_A,
            "buttonEast" => gamepad_B,
            "buttonWest" => gamepad_X,
            "buttonNorth" => gamepad_Y,

            "leftShoulder" => gamepad_LeftBumper,
            "rightShoulder" => gamepad_RightBumper,
            "leftTrigger" => gamepad_LeftTrigger,
            "rightTrigger" => gamepad_RightTrigger,

            "leftStickPress" => gamepad_L3,
            "rightStickPress" => gamepad_R3,

            "startButton" => gamepad_Start,
            "selectButton" or "menuButton" => gamepad_Select,

            // ------------------
            // Mouse
            // ------------------
            "press" => mouseLclick,
            "rightButton" => mouseRclick,
            "middleButton" => mouseMclick,

            "Press LMB" => mouseLclick,
            "Press RMB" => mouseRclick,

            _ => null,
        };
    }


}
