using UnityEngine;

public class Stage_NextStagePortal : MonoBehaviour, IInteractable
{
    public enum PortalType { Normal, Elite, Boss}
    [SerializeField] PortalType nextMapType = PortalType.Normal;

    AudioSource soundPlayer;
    [SerializeField] AudioClip sound_Interact;

    public bool TryInteract()
    {
        if (nextMapType != PortalType.Boss)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<Control_Stage>().StagePlay((int)nextMapType);
        }
        else
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<Control_Stage>().BossStagePlay();
        }

        soundPlayer = GetComponent<AudioSource>();
        soundPlayer.PlayOneShot(sound_Interact);
        return true;
    }

}
