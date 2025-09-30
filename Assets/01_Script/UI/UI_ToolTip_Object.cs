using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_ToolTip_Object : MonoBehaviour
{
    public enum ObjectType { Normal, Weapon, Stat };

    public ObjectType type = ObjectType.Normal;

    [SerializeField] Image img_icon;
    [SerializeField] TextMeshProUGUI text_subName;
    [SerializeField] TextMeshProUGUI text_Desc;
    [SerializeField] TextMeshProUGUI text_Name;

    [SerializeField] TextMeshProUGUI[] text_Stats = new TextMeshProUGUI[9];

    public void SetText(Item_ToolTip toolTip)
    {
        switch (type)
        {
            case ObjectType.Normal:
                text_Name.text = toolTip.title;
                text_Desc.text = toolTip.description;
                break;
            case ObjectType.Weapon:
                text_Name.text = toolTip.title;
                text_subName.text = toolTip.subTitle;
                text_Desc.text = toolTip.description;
                for(int i = 0; i < text_Stats.Length; i++)
                {
                    text_Stats[i].text = toolTip.weaponStat[i];
                }
                break;
            case ObjectType.Stat:
                text_Name.text = toolTip.title;
                text_subName.text = toolTip.subTitle;
                text_Desc.text = toolTip.description;
                break;
            default:
                break;
        }
    }


}
