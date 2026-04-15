using UnityEngine;

public class Tutorial_BossPortal : MonoBehaviour, IInteractable
{

    AudioSource soundPlayer;
    [SerializeField] AudioClip sound_Interact;
    Direction_TutorialTeller teller;

    [SerializeField] GameObject bossStage;


    public bool TryInteract()
    {
        bossStage.SetActive(true);

        soundPlayer = GetComponent<AudioSource>();
        soundPlayer.PlayOneShot(sound_Interact);
        return true;
    }


}
