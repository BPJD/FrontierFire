using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Tutorial_KeyPanel : MonoBehaviour
{
    Animator keyPanelAnicon;
    static string keyPanelAnicon_bool = "isFading";

    public static string[] keyStrings = { "Move", "Move", "Jump", "Interact", "Interact", "Interact", "DownJump", "Interact", "Interact" };
    public static string[] keyStringsWeaponChange = { "PrevWeapon", "NextWeapon", "WeaponA", "WeaponB", "WeaponC" };
    public static string[] keyStringsEngage = { "Aiming", "Attack" };

    public enum Tutorial_SettedKey
    {
        Left,
        Right,
        Jump,
        OpenChest,
        WeaponGet,
        WeaponChange,
        DownJump,
        Engage,
        Portal
    };

    [SerializeField] GameObject[] keyObjs;

    [SerializeField] GameObject[] weaponChangeObjs;

    bool isGamePad = false;

    PlayerInput playerInput;
    UI_InputDeviceDetector inputDeviceDetector;
    DataKeyMapIcons data_keyIcons;
    int prevStep = 0;

    void Start()
    {
        keyPanelAnicon = GetComponentInParent<Animator>();
        playerInput = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).GetComponent<PlayerInput>();
        inputDeviceDetector = GameObject.FindGameObjectWithTag("Module").GetComponent<UI_InputDeviceDetector>();

        data_keyIcons = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag)
            .GetComponent<DataKeyMapIcons>();


        
    }


    private void Update()
    {
        isGamePad = inputDeviceDetector.currentInputType == UI_InputDeviceDetector.InputType.Gamepad;
        int _code = isGamePad ? 1 : 0;

        if (keyObjs[6] != weaponChangeObjs[_code])
        {
            if (keyObjs[6].activeSelf)
            {
                keyObjs[6].SetActive(false);
                weaponChangeObjs[_code].SetActive(true);
            }
            keyObjs[6] = weaponChangeObjs[_code];

                
        }



    }

    public void SetTutorialKey(Tutorial_SettedKey tutorialKey, Image icon, TextMeshProUGUI keyText)
    {
        string key = GetTutorialKeyString(tutorialKey);
        Sprite _icon = data_keyIcons.GetGamepadIcon(key);

        if (icon != null && _icon != null)
        {
            icon.sprite = _icon;
            icon.enabled = true;
            keyText.text = null;
        }
        else
        {
            icon.enabled = false;
            keyText.text = key;
        }
    }

    string GetTutorialKeyString(Tutorial_SettedKey tutorialKey)
    {
        switch (tutorialKey)
        {
            case Tutorial_SettedKey.Left:
                return GetCompositePartDisplay(playerInput, "Move", "left");

            case Tutorial_SettedKey.Right:
                return GetCompositePartDisplay(playerInput, "Move", "right");

            case Tutorial_SettedKey.Jump:
                return GetBindingDisplay(playerInput, "Jump");

            case Tutorial_SettedKey.OpenChest:
                return GetBindingDisplay(playerInput, "Interact");

            case Tutorial_SettedKey.WeaponGet:
                return GetBindingDisplay(playerInput, "Interact");

            case Tutorial_SettedKey.WeaponChange:
                return GetBindingDisplay(playerInput, "NextWeapon");

            case Tutorial_SettedKey.DownJump:
                return GetBindingDisplay(playerInput, "DownJump");

            case Tutorial_SettedKey.Engage:
                return GetBindingDisplay(playerInput, "Attack");

            case Tutorial_SettedKey.Portal:
                return GetBindingDisplay(playerInput, "Interact");
        }

        return string.Empty;
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

            // Composite 본체 / Part는 여기서 제외
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

    string GetCompositePartDisplay(PlayerInput playerInput, string actionName, string partName)
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

            // Composite의 하위 파트만 찾기
            if (!binding.isPartOfComposite)
                continue;

            // left / right / up / down
            if (!string.Equals(binding.name, partName, System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.IsNullOrEmpty(currentScheme) &&
                InputBinding.MaskByGroup(currentScheme).Matches(binding))
            {
                return action.GetBindingDisplayString(i);
            }
        }

        return string.Empty;
    }

    public void StepChanged(int step)
    {
        if (step == 0)
            keyPanelAnicon.SetBool(keyPanelAnicon_bool, false);
        else
            keyPanelAnicon.SetBool(keyPanelAnicon_bool, true);


        for (int i = 0; i < keyObjs.Length; i++)
        {
            keyObjs[i].SetActive(i == step);
        }
    }

    public void SetActionKey(string actionName, Image icon, TextMeshProUGUI keyText)
    {
        string key = GetBindingDisplay(playerInput, actionName);
        Sprite _icon = data_keyIcons.GetGamepadIcon(key);

        if (icon != null && _icon != null)
        {
            icon.sprite = _icon;
            icon.enabled = true;

            keyText.text = null;
        }
        else
        {
            icon.enabled = false;
            keyText.text = key;
        }
    }
}