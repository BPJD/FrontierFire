using Michsky.UI.Heat;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainUI_SettingVideos : MonoBehaviour
{
    [SerializeField] int[] resolutionWidth = { 1280, 1366, 1536, 1600, 1440, 1920, 2560 };
    [SerializeField] int[] resolutionHeight = { 720, 768, 864, 900, 900, 1080, 1440 };

    MainUI_SettingManager settingManager;

    enum DisplayMode
    {
        FullScreen,
        Windowed,
        FullScreenWindow
    }


    DisplayMode selectedDisplayMode = DisplayMode.FullScreen;
    bool selectedIsVSync = false;

    [SerializeField] HorizontalSelector resolutionSelector;
    [SerializeField] HorizontalSelector displayModeSelector;
    [SerializeField] HorizontalSelector qualitySelector;

    [SerializeField] SwitchManager vSyncSwitch;
    [SerializeField] SwitchManager frameRateLimitSwitch;

    [SerializeField] Slider frameRateSlider;
    [SerializeField] GameObject frameRateSliderObj;

    [SerializeField] TextMeshProUGUI frameRateTxt;
    [SerializeField] TMP_InputField frameRateInputTxt;
    [SerializeField] TextMeshProUGUI unlimitTxt;


    UI_InputDeviceDetector inputDetector;
    [SerializeField] GameObject firstSelect;

    int selectedResolutionIndex = 0;
    int selectedFrameRateLimit = 60;
    int selectedQualityIndex = 0;
    bool selectedIsFrameRateLimitSet = false;

    int savedResolutionIndex = 0;
    int savedFrameRateLimit = 60;
    bool savedIsFrameRateLimitSet = false;
    bool savedVSync = false;
    int savedScreenMode = 0;
    int savedQualityIndex = 0;


    private void OnEnable()
    {
        if(inputDetector == null)
        {
            inputDetector = GameObject.FindGameObjectWithTag("Module").GetComponent<UI_InputDeviceDetector>();
        }

        switch (inputDetector.currentInputType)
        {
            case UI_InputDeviceDetector.InputType.Gamepad:
                EventSystem.current.SetSelectedGameObject(firstSelect);
                break;
        }
    }

    public void SettingEnabled()
    {
        if(settingManager == null)
        {
            settingManager = GetComponentInParent<MainUI_SettingManager>();
        }
        
        savedResolutionIndex = ES3.Load<int>("Setting_Resolution", 4);
        savedFrameRateLimit = ES3.Load<int>("Setting_FrameRateLimit", 60);
        savedIsFrameRateLimitSet = ES3.Load<bool>("Setting_IsFrameRateLimit", false);
        savedVSync = ES3.Load<bool>("Setting_VSync", false);
        savedScreenMode = ES3.Load<int>("Setting_ScreenMode", 1);
        savedQualityIndex = ES3.Load<int>("Setting_QualityIndex", 2);

        resolutionSelector.index = savedResolutionIndex;
        selectedResolutionIndex = savedResolutionIndex;

        qualitySelector.index = savedQualityIndex;
        selectedQualityIndex = savedQualityIndex;

        frameRateSlider.value = savedFrameRateLimit;
        selectedFrameRateLimit = savedFrameRateLimit;
        selectedIsFrameRateLimitSet = savedIsFrameRateLimitSet;

        vSyncSwitch.isOn = savedVSync;
        frameRateLimitSwitch.isOn = savedIsFrameRateLimitSet;
        frameRateSliderObj.SetActive(selectedIsFrameRateLimitSet);

        selectedIsVSync = savedVSync;
        selectedDisplayMode = (DisplayMode)savedScreenMode;

        UpdateUI();
    }

    public void ResolutionClicked()
    {
        selectedResolutionIndex = resolutionSelector.index;
        RecalculateChanged();
    }

    public void Slider_FrameRateChanged()
    {
        int fr = Mathf.RoundToInt(frameRateSlider.value);

        selectedFrameRateLimit = fr;

        RecalculateChanged();
    }

    public void FullScreenClicked()
    {
        selectedDisplayMode = (DisplayMode)displayModeSelector.index;
        RecalculateChanged();
    }
    public void SwitchVSyncToggleClicked(bool isOn)
    {
        selectedIsVSync = isOn;
        RecalculateChanged();
    }


    public void QualityButtonClicked()
    {
        selectedQualityIndex = qualitySelector.index;
        RecalculateChanged();
    }

    public void FrameLimitSwitchClicked(bool isOn)
    {
        selectedIsFrameRateLimitSet = isOn;
        frameRateSliderObj.SetActive(isOn);

        RecalculateChanged();
    }


    void RecalculateChanged()
    {
        if (settingManager == null) return;

        bool changed =
            savedResolutionIndex != selectedResolutionIndex ||
            savedFrameRateLimit != selectedFrameRateLimit ||
            savedIsFrameRateLimitSet != selectedIsFrameRateLimitSet ||
            savedVSync != selectedIsVSync ||
            savedScreenMode != (int)selectedDisplayMode ||
            savedQualityIndex != selectedQualityIndex;

        settingManager.isSettingChanged = changed;
    }

    public void ApplyOptions()
    {
        // 1. 해상도
        int width = resolutionWidth[selectedResolutionIndex];
        int height = resolutionHeight[selectedResolutionIndex];

        FullScreenMode _screenMode = FullScreenMode.ExclusiveFullScreen;

        switch(selectedDisplayMode)
        {
            case DisplayMode.FullScreen:
                _screenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case DisplayMode.Windowed:
                _screenMode = FullScreenMode.Windowed;
                break;
            case DisplayMode.FullScreenWindow:
                _screenMode = FullScreenMode.FullScreenWindow;
                break;
            default:
                break;
        };

        Screen.SetResolution(width, height, _screenMode);

        // 2. VSync
        // vSync ON = 1, OFF = 0
        QualitySettings.vSyncCount = selectedIsVSync ? 1 : 0;

        // 3. 프레임 레이트 제한
        if (selectedIsFrameRateLimitSet)
        {
            Application.targetFrameRate = selectedFrameRateLimit;
        }
        else
        {
            Application.targetFrameRate = -1; // 무제한
        }

        // 4. 그래픽 품질
        QualitySettings.SetQualityLevel(selectedQualityIndex, true);

        // 5. 저장
        ES3.Save("Setting_Resolution", selectedResolutionIndex);
        ES3.Save("Setting_FrameRateLimit", selectedFrameRateLimit);
        ES3.Save("Setting_IsFrameRateLimit", selectedIsFrameRateLimitSet);
        ES3.Save("Setting_VSync", selectedIsVSync);
        ES3.Save("Setting_ScreenMode", (int)selectedDisplayMode);
        ES3.Save("Setting_QualityIndex", selectedQualityIndex);

        // 6. saved 값 동기화 (중요)
        savedResolutionIndex = selectedResolutionIndex;
        savedFrameRateLimit = selectedFrameRateLimit;
        savedIsFrameRateLimitSet = selectedIsFrameRateLimitSet;
        savedVSync = selectedIsVSync;
        savedScreenMode = (int)selectedDisplayMode;
        savedQualityIndex = selectedQualityIndex;

        // 7. 변경 상태 리셋
        if (settingManager != null)
        {
            settingManager.isSettingChanged = false;
        }
    }


    void UpdateUI()
    {
        resolutionSelector.UpdateUI();
        displayModeSelector.UpdateUI();
        qualitySelector.UpdateUI();

        vSyncSwitch.UpdateUI();
        frameRateLimitSwitch.UpdateUI();

        frameRateSlider.value = savedFrameRateLimit;
    }
}
