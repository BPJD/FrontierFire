using Michsky.UI.Heat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_NormalToolTipTextSet : MonoBehaviour
{
    enum SettedAction { None, Interact, Teleport, NextStage, OpenBox, ItemGet }
    [SerializeField] SettedAction action = SettedAction.None;

    enum SettedKey { None, Interact, HideWeapon }
    static string[] keyStrings = { "None", "Interact", "HideWeaponInfo" };
    static string localizeKey = "ToolTipKey_";

    [SerializeField] SettedKey settedKey = SettedKey.Interact;

    Item_ToolTip toolTip;
    string key;
    string desc;
    int keyCode = 0;

    PlayerInput playerInput;
    UI_InputDeviceDetector inputDeviceDetector;

    DataKeyMapIcons data_keyIcons;

    LocalizedObject localize;


    private void Start()
    {
        toolTip = GetComponent<Item_ToolTip>();
        playerInput = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).GetComponent<PlayerInput>();
        inputDeviceDetector = GameObject.FindGameObjectWithTag("Module").GetComponent<UI_InputDeviceDetector>();

        if (settedKey != SettedKey.None)
        {
            data_keyIcons = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag)
                .GetComponent<DataKeyMapIcons>();

            keyCode = (int)settedKey;
            key = GetBindingDisplay(playerInput, keyStrings[keyCode]);
            toolTip.title = key;

            if (action != SettedAction.None)
                SetText(localizeKey + action.ToString());

            Image _Icon = toolTip.toolTip_icon;
            if (_Icon != null)
            {
                bool _isGamePad =
                    inputDeviceDetector.currentInputType == UI_InputDeviceDetector.InputType.Gamepad;

                if (_isGamePad)
                {
                    _Icon.sprite = data_keyIcons.GetGamepadIcon(key);
                    _Icon.enabled = true;
                }
                else
                {
                    _Icon.enabled = false;
                }
            }
        }

    }


    string GetBindingDisplay(PlayerInput playerInput, string actionName)
    {
        if (playerInput == null || playerInput.actions == null)
            return string.Empty;

        var action = playerInput.actions.FindAction(actionName, true);
        if (action == null)
            return string.Empty;

        string currentScheme = playerInput.currentControlScheme;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (binding.isComposite || binding.isPartOfComposite)
                continue;

            if (!string.IsNullOrEmpty(currentScheme) &&
                InputBinding.MaskByGroup(currentScheme).Matches(binding))
            {
                return action.GetBindingDisplayString(i);
            }
        }

        return action.GetBindingDisplayString();
    }


    void SetText(string key)
    {
        localize = GetComponent<LocalizedObject>();

        localize.localizationKey = key;
        localize.UpdateItem();
    }
}


