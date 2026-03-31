using UnityEngine;
using UnityEngine.Audio;
using Michsky.UI.Heat;

public class PlayerSettingLoader : MonoBehaviour
{
    [Header("Optional")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private LocalizationManager localizationBridge;
    // 직접 LocalizationManager를 참조하기 어렵다면 브리지용 래퍼를 하나 둬도 됨

    [Header("Audio Mixer Params")]
    [SerializeField]
    private string[] volumeParamNames = new string[5]
    {
        "Vol_Master",
        "Vol_Music",
        "Vol_SFX",
        "Vol_Ambient",
        "Vol_UI"
    };

    private const float MUTE_DB = -80f;

    private readonly string[] languageCodes =
    {
        "de-DE", "en-US", "es-ES", "fr-FR", "ja-JP",
        "ko-KR", "pl-PL", "pt-BR", "ru-RU", "tr-TR", "zh-CN"
    };

    private void Awake()
    {
        ApplyVideoSettings();
        ApplyAudioSettings();
        ApplyLanguageSettings();
    }

    private void ApplyVideoSettings()
    {
        int resolutionIndex = ES3.Load<int>("Setting_Resolution", 4);
        int frameRateLimit = ES3.Load<int>("Setting_FrameRateLimit", 60);
        bool isFrameRateLimitSet = ES3.Load<bool>("Setting_IsFrameRateLimit", false);
        bool vSync = ES3.Load<bool>("Setting_VSync", false);
        int screenMode = ES3.Load<int>("Setting_ScreenMode", 1);
        int qualityIndex = ES3.Load<int>("Setting_QualityIndex", 2);

        int[] resolutionWidth = { 1280, 1366, 1536, 1600, 1440, 1920, 2560 };
        int[] resolutionHeight = { 720, 768, 864, 900, 900, 1080, 1440 };

        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutionWidth.Length - 1);

        FullScreenMode mode = FullScreenMode.Windowed;
        switch (screenMode)
        {
            case 0: mode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: mode = FullScreenMode.Windowed; break;
            case 2: mode = FullScreenMode.FullScreenWindow; break;
        }

        Screen.SetResolution(resolutionWidth[resolutionIndex], resolutionHeight[resolutionIndex], mode);
        QualitySettings.vSyncCount = vSync ? 1 : 0;
        Application.targetFrameRate = isFrameRateLimitSet ? frameRateLimit : -1;
        QualitySettings.SetQualityLevel(qualityIndex, true);
    }

    private void ApplyAudioSettings()
    {
        if (audioMixer == null) return;

        int[] volumes =
        {
            ES3.Load("Setting_Volume_Master", 70),
            ES3.Load("Setting_Volume_BGM", 70),
            ES3.Load("Setting_Volume_SFX", 70),
            ES3.Load("Setting_Volume_Ambient", 70),
            ES3.Load("Setting_Volume_UI", 70)
        };

        bool[] mutes =
        {
            ES3.Load("Setting_isMute_Master", false),
            ES3.Load("Setting_isMute_BGM", false),
            ES3.Load("Setting_isMute_SFX", false),
            ES3.Load("Setting_isMute_Ambient", false),
            ES3.Load("Setting_isMute_UI", false)
        };

        for (int i = 0; i < volumeParamNames.Length; i++)
        {
            float db = VolumeToDb(volumes[i], mutes[i]);
            audioMixer.SetFloat(volumeParamNames[i], db);
        }
    }

    private void ApplyLanguageSettings()
    {
        int languageIndex = ES3.Load<int>("Setting_Language", 5); // ko-KR 기본값 예시
        languageIndex = Mathf.Clamp(languageIndex, 0, languageCodes.Length - 1);

        if (localizationBridge != null)
        {
            localizationBridge.SetLanguage(languageCodes[languageIndex]);
        }
    }

    private float VolumeToDb(int volume, bool isMute)
    {
        if (isMute || volume <= 0) return MUTE_DB;
        return Mathf.Log10(volume / 100f) * 20f;
    }
}