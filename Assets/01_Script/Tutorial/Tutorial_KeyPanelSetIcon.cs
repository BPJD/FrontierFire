using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial_KeyPanelSetIcon : MonoBehaviour
{
    Tutorial_KeyPanel tutorial_KeyPanel;

    [SerializeField] Tutorial_KeyPanel.Tutorial_SettedKey settedKey = Tutorial_KeyPanel.Tutorial_SettedKey.Left;
    [SerializeField] Image keyIcon;
    [SerializeField] TextMeshProUGUI keyText;

    [SerializeField] int code = 0;

    void Start()
    {
        tutorial_KeyPanel = GetComponentInParent<Tutorial_KeyPanel>();

        if (tutorial_KeyPanel == null || keyText == null)
            return;

        if (settedKey == Tutorial_KeyPanel.Tutorial_SettedKey.WeaponChange)
        {
            if (code >= 0 && code < Tutorial_KeyPanel.keyStringsWeaponChange.Length)
            {
                tutorial_KeyPanel.SetActionKey(
                    Tutorial_KeyPanel.keyStringsWeaponChange[code],
                    keyIcon,
                    keyText
                );
            }
            return;
        }

        if (settedKey == Tutorial_KeyPanel.Tutorial_SettedKey.Engage)
        {
            if (code >= 0 && code < Tutorial_KeyPanel.keyStringsEngage.Length)
            {
                tutorial_KeyPanel.SetActionKey(
                    Tutorial_KeyPanel.keyStringsEngage[code],
                    keyIcon,
                    keyText
                );
            }
            return;
        }

        tutorial_KeyPanel.SetTutorialKey(settedKey, keyIcon, keyText);
    }
}