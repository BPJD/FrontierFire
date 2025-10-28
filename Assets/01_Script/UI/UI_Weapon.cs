using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Weapon : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    [SerializeField] Image[] weaponImgs = new Image[3];
    [SerializeField] Image[] weaponFrames = new Image[3];
    
    public void SetImageIcon(int slot, Sprite icon)
    {
        weaponImgs[slot].sprite = icon;
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
