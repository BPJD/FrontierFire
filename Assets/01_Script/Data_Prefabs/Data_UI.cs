using UnityEngine;

public class Data_UI : MonoBehaviour
{
    [SerializeField] Sprite[] weaponIcons;

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



}
