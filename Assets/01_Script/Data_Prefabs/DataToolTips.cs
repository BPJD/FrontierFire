using UnityEngine;

public class DataToolTips : MonoBehaviour
{
    [SerializeField] RectTransform normal, item, weapon;

    public RectTransform GetToolTipData(UI_ToolTip_Object.ObjectType type)
    {
        switch (type)
        {
            case UI_ToolTip_Object.ObjectType.Weapon:
                return weapon;
            case UI_ToolTip_Object.ObjectType.Normal:
                return normal;
            case UI_ToolTip_Object.ObjectType.Stat:
                return item;
        }
        return null;
    }
}
