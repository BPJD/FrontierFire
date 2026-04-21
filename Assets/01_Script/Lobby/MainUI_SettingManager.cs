using UnityEngine;
using UnityEngine.EventSystems;

public class MainUI_SettingManager : MonoBehaviour
{

    public bool isSettingChanged = false;

    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject settingPanel;
    [SerializeField] GameObject saveConfirmPanel;


    [SerializeField] MainUI_SettingVideos settingVideo;
    [SerializeField] MainUI_SettingAudios settingAudio;
    [SerializeField] MainUI_SettingKeyMapping[] settingKeyMappings;
    [SerializeField] MainUI_SettingGeneral settingGeneral;

    [SerializeField] GameObject firstButton;

    UI_SoundPlayer uiSoundPlayer;

    private void Start()
    {
        settingGeneral.SettingEnabled();
        uiSoundPlayer = GetComponentInChildren<UI_SoundPlayer>();
    }

    public void Button_SettingOpen()
    {
        mainPanel.SetActive(false);
        settingPanel.SetActive(true);

        settingAudio.SettingEnabled();
        settingVideo.SettingEnabled();
        for(int i = 0; i < settingKeyMappings.Length; i++)
        {
            settingKeyMappings[i].ForceRefreshLabel();
        }
        settingGeneral.SettingEnabled();

        EventSystem.current.SetSelectedGameObject(firstButton);

        if (uiSoundPlayer != null)
        {
            uiSoundPlayer.PlayUIClickSound();
        }
    }

    public void Button_BackToMenuRequested()
    {

        if (isSettingChanged)
        {
            saveConfirmPanel.SetActive(true);
            if (uiSoundPlayer != null)
            {
                uiSoundPlayer.PlayUINotiOn();
            }
        }
        else
        {
            mainPanel.SetActive(true);
            settingPanel.SetActive(false);
            if (uiSoundPlayer != null)
            {
                uiSoundPlayer.PlayUIClickSound();
            }
        }
    }

    public void Button_SettingConfirmClicked()
    {
        if (uiSoundPlayer != null)
        {
            uiSoundPlayer.PlayUIClickSound();
        }
        SaveOptions();
    }

    public void Button_SettingConfirmClose(bool isSave)
    {
        saveConfirmPanel.SetActive(false);

        if (isSave)
        {
            SaveOptions();
            if (uiSoundPlayer != null)
            {
                uiSoundPlayer.PlayUIConfirm();
            }
        }
        else
        {
            mainPanel.SetActive(true);
            settingPanel.SetActive(false);
            if (uiSoundPlayer != null)
            {
                uiSoundPlayer.PlayUINotiOff();
            }
        }
    }

    void SaveOptions()
    {
        //Save Options

        settingAudio.ApplyOptions();
        settingVideo.ApplyOptions();

        isSettingChanged = false;

        mainPanel.SetActive(true);
        settingPanel.SetActive(false);
    }
}
