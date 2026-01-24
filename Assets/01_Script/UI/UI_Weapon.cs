using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Weapon : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    [SerializeField] Image[] weaponImgs = new Image[3];
    [SerializeField] Image[] weaponFrames = new Image[3];
    [SerializeField] Sprite nullIcon;
    
    public void SetImageIcon(int slot, Sprite icon)
    {
        if (icon != null)
        {
            weaponImgs[slot].sprite = icon;
        }
        else
        {
            weaponImgs[slot].sprite = nullIcon;
        }

    }

    public void SetWeaponSelectedUI(int slot, bool isOn)
    {
        Color _color = Color.white;

        if (isOn) 
        {
            _color = Color.yellow;
        }

        weaponFrames[slot].color = _color;
    }
}
