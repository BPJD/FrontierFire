using UnityEngine;

public class Stage_NextStagePortal : MonoBehaviour, IInteractable
{
    public enum PortalType { Normal, Elite, Boss}
    [SerializeField] PortalType nextMapType = PortalType.Normal;

    public void Interact()
    {
        if (nextMapType != PortalType.Boss)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<Control_Stage>().StagePlay((int)nextMapType);
        }
        else
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<Control_Stage>().BossStagePlay();
        }
    }

}
