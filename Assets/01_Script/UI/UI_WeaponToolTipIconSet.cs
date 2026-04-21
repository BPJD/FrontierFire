using Michsky.UI.Heat;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_WeaponToolTipIconSet : MonoBehaviour
{
    static string[] keyStrings = { "None", "Interact", "HideWeaponInfo" };

    [SerializeField] UI_NormalToolTipTextSet.SettedKey settedKey = UI_NormalToolTipTextSet.SettedKey.Interact;

    string key;

    PlayerInput playerInput;
    UI_InputDeviceDetector inputDeviceDetector;

    DataKeyMapIcons data_keyIcons;

    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI keyText;

    [SerializeField] string customKeyString = "";


    void Start()
    {

        playerInput = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).GetComponent<PlayerInput>();
        inputDeviceDetector = GameObject.FindGameObjectWithTag("Module").GetComponent<UI_InputDeviceDetector>();

        if (settedKey != UI_NormalToolTipTextSet.SettedKey.None)
        {
            data_keyIcons = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag)
                .GetComponent<DataKeyMapIcons>();

            key = GetBindingDisplay(playerInput, keyStrings[(int)settedKey]);
            keyText.text = key;

            
            if (icon != null)
            {
                bool _isGamePad =
                    inputDeviceDetector.currentInputType == UI_InputDeviceDetector.InputType.Gamepad;

                if (_isGamePad)
                {
                    icon.sprite = data_keyIcons.GetGamepadIcon(key);
                    icon.enabled = true;
                }
                else
                {
                    icon.enabled = false;
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

}


