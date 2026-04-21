using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainUI_DifficultySettingKey : MonoBehaviour
{
    public enum SettedKey
    {
        Up,
        Down,
        Left,
        Right,
        Jump,
        Interact,
        DownJump,
        Attack,
        Aiming,
        PrevWeapon,
        NextWeapon,
        WeaponA,
        WeaponB,
        WeaponC
    }

    [Header("Target")]
    [SerializeField] private SettedKey settedKey = SettedKey.Jump;

    [Header("UI")]
    [SerializeField] private Image keyIcon;
    [SerializeField] private TextMeshProUGUI keyText;

    [Header("Refs")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private DataKeyMapIcons dataKeyIcons;

    private void Start()
    {
        TryFindRefs();
        RefreshKey();
    }

    public void RefreshKey()
    {
        TryFindRefs();

        if (playerInput == null || keyText == null)
            return;

        string key = GetKeyString(settedKey);
        Sprite icon = null;

        if (dataKeyIcons != null && !string.IsNullOrEmpty(key))
            icon = dataKeyIcons.GetGamepadIcon(key);

        if (keyIcon != null && icon != null)
        {
            keyIcon.sprite = icon;
            keyIcon.enabled = true;
            keyText.text = "";
        }
        else
        {
            if (keyIcon != null)
                keyIcon.enabled = false;

            keyText.text = key;
        }
    }

    private void TryFindRefs()
    {
        if (playerInput == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
            if (player != null)
                playerInput = player.GetComponent<PlayerInput>();
        }

        if (dataKeyIcons == null)
        {
            GameObject dataObj = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag);
            if (dataObj != null)
                dataKeyIcons = dataObj.GetComponent<DataKeyMapIcons>();
        }
    }

    private string GetKeyString(SettedKey keyType)
    {
        switch (keyType)
        {
            case SettedKey.Up:
                return GetCompositePartDisplay("Move", "up");

            case SettedKey.Down:
                return GetCompositePartDisplay("Move", "down");

            case SettedKey.Left:
                return GetCompositePartDisplay("Move", "left");

            case SettedKey.Right:
                return GetCompositePartDisplay("Move", "right");

            case SettedKey.Jump:
                return GetBindingDisplay("Jump");

            case SettedKey.Interact:
                return GetBindingDisplay("Interact");

            case SettedKey.DownJump:
                return GetBindingDisplay("DownJump");

            case SettedKey.Attack:
                return GetBindingDisplay("Attack");

            case SettedKey.Aiming:
                return GetBindingDisplay("Aiming");

            case SettedKey.PrevWeapon:
                return GetBindingDisplay("PrevWeapon");

            case SettedKey.NextWeapon:
                return GetBindingDisplay("NextWeapon");

            case SettedKey.WeaponA:
                return GetBindingDisplay("WeaponA");

            case SettedKey.WeaponB:
                return GetBindingDisplay("WeaponB");

            case SettedKey.WeaponC:
                return GetBindingDisplay("WeaponC");
        }

        return string.Empty;
    }

    private string GetBindingDisplay(string actionName)
    {
        if (playerInput == null || playerInput.actions == null)
            return string.Empty;

        InputAction action = playerInput.actions.FindAction(actionName, true);
        if (action == null)
            return string.Empty;

        string currentScheme = playerInput.currentControlScheme;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];

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

    private string GetCompositePartDisplay(string actionName, string partName)
    {
        if (playerInput == null || playerInput.actions == null)
            return string.Empty;

        InputAction action = playerInput.actions.FindAction(actionName, true);
        if (action == null)
            return string.Empty;

        string currentScheme = playerInput.currentControlScheme;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];

            if (!binding.isPartOfComposite)
                continue;

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

    public void SetPlayerInput(PlayerInput input)
    {
        playerInput = input;
        RefreshKey();
    }
}