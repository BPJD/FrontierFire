using UnityEngine;
using Michsky.UI.Heat;

public class UI_NotificationTextSet : MonoBehaviour
{
    LocalizedObject localize;

    public void SetText(string key)
    {
        localize = GetComponentInChildren<LocalizedObject>();

        localize.localizationKey = key;
        localize.UpdateItem();
    }

    private void Start()
    {
        Destroy(gameObject, 8f);
    }
}
