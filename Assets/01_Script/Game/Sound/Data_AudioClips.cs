using UnityEngine;

public class Data_AudioClips : MonoBehaviour
{
    [SerializeField] AudioClip[] sounds_PortalOpen;


    public AudioClip GetPortalSoundClipByPortalType(int stageType)
    {
        //일반 0, 정예 1, 보스 2
        return sounds_PortalOpen[stageType];
    }
}
