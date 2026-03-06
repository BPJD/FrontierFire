using UnityEngine;
using UnityEngine.EventSystems;

public class MainUI_SettingManager : MonoBehaviour
{

    public bool isSettingChanged = false;

    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject settingPanel;
    [SerializeField] GameObject saveConfirmPanel;


    [SerializeField] MainUI_SettingVideos settingVideo;
    [SerializeField] MainUi_SettingAudios settingAudio;
    [SerializeField] MainUI_SettingKeyMapping[] settingKeyMappings;
    [SerializeField] MainUI_SettingGeneral settingGeneral;

    [SerializeField] GameObject firstButton;

    private void Start()
    {
        settingGeneral.SettingEnabled();
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
    }

    public void Button_BackToMenuRequested()
    {
        if (isSettingChanged)
        {
            saveConfirmPanel.SetActive(true);
        }
        else
        {
            mainPanel.SetActive(true);
            settingPanel.SetActive(false);
        }
    }

    public void Button_SettingConfirmClicked()
    {
        SaveOptions();
    }
    public void Button_SettingConfirmClose(bool isSave)
    {
        saveConfirmPanel.SetActive(false);

        if (isSave)
        {
            SaveOptions();
        }
        else
        {
            mainPanel.SetActive(true);
            settingPanel.SetActive(false);
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
