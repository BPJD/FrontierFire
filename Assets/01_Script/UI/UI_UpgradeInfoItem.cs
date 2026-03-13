using TMPro;
using UnityEngine;
using Michsky.UI.Heat;

public class UI_UpgradeInfoItem : MonoBehaviour
{
    [SerializeField] LocalizedObject itemEftName;
    [SerializeField] TextMeshProUGUI itemEftValue;

    [SerializeField] Color upColor;
    [SerializeField] Color downColor;

    public void SetUpgradeInfo(string eftName, string eftValue, int statId)
    {
        itemEftName.localizationKey = eftName;
        itemEftName.UpdateItem();

        itemEftValue.text = eftValue;
        itemEftValue.color = ColorSet(eftValue, statId);
    }

    Color ColorSet(string value, int statId)
    {
        bool isPositive = value.StartsWith("+");
        bool isNegative = value.StartsWith("-");
        bool reverse = IsLowerValueBetter(statId);

        if (!isPositive && !isNegative)
            return Color.white;

        if (!reverse)
        {
            if (isPositive) return upColor;
            if (isNegative) return downColor;
        }
        else
        {
            if (isPositive) return downColor;
            if (isNegative) return upColor;
        }

        return Color.white;
    }

    bool IsLowerValueBetter(int statId)
    {
        switch (statId)
        {
            case 3:   // 예시: 받는 피해
                return true;

            default:
                return false;
        }
    }
}