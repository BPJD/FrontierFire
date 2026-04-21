using UnityEngine;

public class UI_SoundPlayer : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] Data_UI data_ui;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (data_ui == null)
        {
            data_ui = GameObject.FindGameObjectWithTag(Data_Strings.DataObjTag).GetComponent<Data_UI>();
        }

    }




    public void PlayUIHoverSound()
    {
        if (audioSource != null && data_ui != null)
        {
            audioSource.PlayOneShot(data_ui.soundBtnHover);
        }
    }
    public void PlayUIClickSound()
    {
        if (audioSource != null && data_ui != null)
        {
            audioSource.PlayOneShot(data_ui.soundBtnPressed);
        }
    }

    public void PlayUINotiOn()
    {
        if (audioSource != null && data_ui != null)
        {
            audioSource.PlayOneShot(data_ui.soundBtnNotiOn);
        }
    }
    public void PlayUINotiOff()
    {
        if (audioSource != null && data_ui != null)
        {
                audioSource.PlayOneShot(data_ui.soundBtnNotiOff);
        }
    }

    public void PlayUIConfirm()
    {
        if (audioSource != null && data_ui != null)
        {
            audioSource.PlayOneShot(data_ui.soundBtnConfirm);
        }
    }
    public void PlayUIDenied()
    {
        if (audioSource != null && data_ui != null)
        {
            audioSource.PlayOneShot(data_ui.soundBtnDenied);
        }
    }
}
