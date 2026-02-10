using UnityEngine;
using UnityEngine.UI;

public class MainUI_SettingTabs : MonoBehaviour
{


    [SerializeField] GameObject[] tabPanels;
    [SerializeField] GameObject panelSettingConfirm;

    [SerializeField] Button[] tabButtons;

    [SerializeField] UITabIndicator tabIndicator;



    public void TabButtonClicked(int code)
    {
        for (int i = 0; i < tabPanels.Length; i++)
        {
            if (i == code)
            {
                tabPanels[i].SetActive(true);
                tabIndicator.AnimateTo(tabButtons[i].GetComponent<RectTransform>());
            }
            else
            {
                tabPanels[i].SetActive(false);
            }
        }
    }


}
