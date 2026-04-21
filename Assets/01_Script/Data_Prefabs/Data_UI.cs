using UnityEngine;
using UnityEngine.UI;

public class Data_UI : MonoBehaviour
{
    [SerializeField] Sprite[] weaponIcons;
    [SerializeField] Image playerDashCooldown;

    public AudioClip soundBtnHover;
    public AudioClip soundBtnPressed;
    public AudioClip soundBtnConfirm;
    public AudioClip soundBtnDenied;
    public AudioClip soundBtnNotiOn;
    public AudioClip soundBtnNotiOff;

    public Sprite GetImageByWeaponType(int weaponType, Sprite icon)
    {
        if(icon == null)
        {
            return weaponIcons[weaponType];
        }
        else
        {
            return icon;
        }
        
    }

    public Image GetPlayerDashCooltime()
    {
        return playerDashCooldown;
    }


}
