using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainUI_SettingTabs : MonoBehaviour
{


    [SerializeField] GameObject[] tabPanels;
    [SerializeField] GameObject panelSettingConfirm;

    [SerializeField] Button[] tabButtons;

    [SerializeField] UITabIndicator tabIndicator;

    [SerializeField] GameObject[] firstButtonsInTab;


    public void TabButtonClicked(int code)
    {
        for (int i = 0; i < tabPanels.Length; i++)
        {
            if (i == code)
            {
                tabPanels[i].SetActive(true);
                tabIndicator.AnimateTo(tabButtons[i].GetComponent<RectTransform>());
                EventSystem.current.SetSelectedGameObject(firstButtonsInTab[i]);
            }
            else
            {
                tabPanels[i].SetActive(false);
            }
        }
    }


}
