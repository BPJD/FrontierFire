using UnityEngine;

public class UI_ToolTipKey : MonoBehaviour
{

    UI_ToolTip_Object toolTip;
    CanvasGroup canvasGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        toolTip = GetComponentInParent<UI_ToolTip_Object>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    
    void Update()
    {
        if(toolTip != null)
        {
            if(toolTip.isThisSelected)
            {
                canvasGroup.alpha = 1f;
            }
            else
            {
                canvasGroup.alpha = 0f;
            }
        }
    }
}
