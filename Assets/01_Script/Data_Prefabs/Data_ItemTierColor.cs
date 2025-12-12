
using UnityEngine;

public class Data_ItemTierColor : MonoBehaviour
{
    [SerializeField] Color D, C, B, A, S, SS;

    public Color GetItemTierColor(int value, bool isWeapon)
    {
        int _finalValue = value;

        if (isWeapon)
        {
            _finalValue = WeaponQualityToTier(value);
        }

        return _finalValue switch
        {
            0 => D,
            1 => C,
            2 => B,
            3 => A,
            4 => S,
            5 => SS,
            _ => D,
        };
    }

    public string ReturnItemTier(int value, bool isWeapon)
    {
        int _finalValue = value;

        if (isWeapon)
        {
            _finalValue = WeaponQualityToTier(value);
        }

        return _finalValue switch
        {
            0 => "D",
            1 => "C",
            2 => "B",
            3 => "A",
            4 => "S",
            5 => "SS",
            _ => "Unknown Tier",
        };

    }


    int WeaponQualityToTier(int quality)
    {
        switch (quality)
        {
            case < 30:
                return 0;
            case < 55:
                return 1;
            case < 65:
                return 2;
            case < 75:
                return 3;
            case < 90:
                return 4;
            default:
                return 5;
        }


    }

}
