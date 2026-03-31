using UnityEngine;

public class DataToolTips : MonoBehaviour
{
    [SerializeField] RectTransform normal, item, weapon, weaponUp, ability;

    public RectTransform GetToolTipData(UI_ToolTip_Object.ObjectType type)
    {
        switch (type)
        {
            case UI_ToolTip_Object.ObjectType.Weapon:
                return weapon;
            case UI_ToolTip_Object.ObjectType.Normal:
                return normal;
            case UI_ToolTip_Object.ObjectType.StatUp:
                return item;
            case UI_ToolTip_Object.ObjectType.WeaponUp:
                return weaponUp;
            case UI_ToolTip_Object.ObjectType.Ability:
                return ability;
        }
        return null;
    }
}
