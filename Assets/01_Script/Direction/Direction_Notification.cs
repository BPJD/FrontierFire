using UnityEngine;

public class Direction_Notification : MonoBehaviour
{
    [SerializeField] GameObject notiObj;
    [SerializeField] GameObject panelUI;
    UI_NotificationTextSet notiText;


    public void Notification(string tableKey)
    {
        GameObject _noti = Instantiate(notiObj, panelUI.transform);

        notiText = _noti.GetComponent<UI_NotificationTextSet>();
        notiText.SetText(tableKey);
    }
}
