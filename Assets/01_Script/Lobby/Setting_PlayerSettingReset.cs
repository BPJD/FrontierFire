using Michsky.UI.Heat;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

public class Setting_PlayerSettingReset : MonoBehaviour
{
    [Header("Setting UI References")]
    [SerializeField] private MainUI_SettingVideos settingVideo;
    [SerializeField] private MainUI_SettingAudios settingAudio;
    [SerializeField] private MainUI_SettingGeneral settingGeneral;

    [Header("Optional Runtime Reference")]
    [SerializeField] private PlayerInput currentPlayerInput;

    private UI_SoundPlayer soundPlayer;

    // -----------------------
    // ES3 Keys (General)
    // -----------------------
    private const string KEY_FIRST_RUN_DONE = "Player_FirstRunDone";
    public const string KEY_PLAY_COUNT = "Player_PlayCount";
    public const string KEY_TUTORIAL_CLEAR = "Player_TutorialClear";

    // -----------------------
    // ES3 Keys (Audio)
    // -----------------------
    private const string KEY_VOL_MASTER = "Setting_Volume_Master";
    private const string KEY_VOL_BGM = "Setting_Volume_BGM";
    private const string KEY_VOL_SFX = "Setting_Volume_SFX";
    private const string KEY_VOL_AMBIENT = "Setting_Volume_Ambient";
    private const string KEY_VOL_UI = "Setting_Volume_UI";

    private const string KEY_MUTE_MASTER = "Setting_isMute_Master";
    private const string KEY_MUTE_BGM = "Setting_isMute_BGM";
    private const string KEY_MUTE_SFX = "Setting_isMute_SFX";
    private const string KEY_MUTE_AMBIENT = "Setting_isMute_Ambient";
    private const string KEY_MUTE_UI = "Setting_isMute_UI";

    // -----------------------
    // ES3 Keys (Video)
    // -----------------------
    private const string KEY_RESOLUTION_INDEX = "Setting_Resolution";
    private const string KEY_FPS_LIMIT = "Setting_FrameRateLimit";
    private const string KEY_IS_FPS_LIMIT = "Setting_IsFrameRateLimit";
    private const string KEY_VSYNC = "Setting_VSync";
    private const string KEY_SCREEN_MODE = "Setting_ScreenMode";
    private const string KEY_QUALITY_INDEX = "Setting_QualityIndex";

    // -----------------------
    // ES3 Keys (General)
    // -----------------------
    private const string KEY_LANGUAGE = "Setting_Language";

    private enum KeyboardLayoutPreset
    {
        QWERTY,
        AZERTY
    }

    private void Awake()
    {
        bool firstRunDone = ES3.Load(KEY_FIRST_RUN_DONE, false);

        if (!firstRunDone)
        {
            ResetPlayerSettings();
            ES3.Save(KEY_FIRST_RUN_DONE, true);
        }

        int playCount = ES3.Load(KEY_PLAY_COUNT, 0);
        playCount++;
        ES3.Save(KEY_PLAY_COUNT, playCount);

        soundPlayer = GetComponent<UI_SoundPlayer>();

        if (currentPlayerInput == null)
            currentPlayerInput = FindFirstObjectByType<PlayerInput>();
    }

    /// <summary>
    /// 플레이어 설정 초기화(오디오/비디오/일반/조작)
    /// 저장값만 초기화한다. 실제 런타임 반영은 ResetAndApply()에서 처리한다.
    /// </summary>
    public void ResetPlayerSettings()
    {
        ResetAudioDefaults();
        ResetVideoDefaults();
        ResetGeneralDefaults();
        ResetControlDefaults();
    }

    private static void ResetAudioDefaults()
    {
        ES3.Save(KEY_VOL_MASTER, 70);
        ES3.Save(KEY_VOL_BGM, 40);
        ES3.Save(KEY_VOL_SFX, 70);
        ES3.Save(KEY_VOL_AMBIENT, 70);
        ES3.Save(KEY_VOL_UI, 70);

        ES3.Save(KEY_MUTE_MASTER, false);
        ES3.Save(KEY_MUTE_BGM, false);
        ES3.Save(KEY_MUTE_SFX, false);
        ES3.Save(KEY_MUTE_AMBIENT, false);
        ES3.Save(KEY_MUTE_UI, false);
    }

    private static void ResetVideoDefaults()
    {
        ES3.Save(KEY_RESOLUTION_INDEX, 5);
        ES3.Save(KEY_FPS_LIMIT, 60);
        ES3.Save(KEY_IS_FPS_LIMIT, false);
        ES3.Save(KEY_VSYNC, false);
        ES3.Save(KEY_SCREEN_MODE, 1);
        ES3.Save(KEY_QUALITY_INDEX, 2);
    }

    private static void ResetGeneralDefaults()
    {
        string detectedLanguage = DetectSystemLanguageCode();
        string mappedLanguage = MapToSupportedLanguage(detectedLanguage);

        ES3.Save(KEY_LANGUAGE, mappedLanguage);
    }

    private static void ResetControlDefaults()
    {
        var loader = MainUI_KeyMapLoader.GetOrFind();
        if (loader == null || loader.Actions == null)
            return;

        loader.ResetToDefault();

        var moveAction = loader.Actions.FindAction("Move", throwIfNotFound: false);
        if (moveAction == null)
        {
            Debug.LogWarning("[SettingsReset] Move action not found.");
            loader.Save();
            return;
        }

        KeyboardLayoutPreset preset = DetectKeyboardLayoutPreset();

        switch (preset)
        {
            case KeyboardLayoutPreset.AZERTY:
                ApplyMoveComposite(moveAction,
                    "<Keyboard>/z",
                    "<Keyboard>/s",
                    "<Keyboard>/q",
                    "<Keyboard>/d");
                break;

            case KeyboardLayoutPreset.QWERTY:
            default:
                ApplyMoveComposite(moveAction,
                    "<Keyboard>/w",
                    "<Keyboard>/s",
                    "<Keyboard>/a",
                    "<Keyboard>/d");
                break;
        }

        loader.Save();
    }

    private static KeyboardLayoutPreset DetectKeyboardLayoutPreset()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return KeyboardLayoutPreset.QWERTY;

        string a = keyboard.aKey.displayName?.ToUpperInvariant();
        string w = keyboard.wKey.displayName?.ToUpperInvariant();

        if (a == "Q" && w == "Z")
            return KeyboardLayoutPreset.AZERTY;

        return KeyboardLayoutPreset.QWERTY;
    }

    private static void ApplyMoveComposite(InputAction action, string up, string down, string left, string right)
    {
        int upIndex = FindCompositePartBindingIndex(action, "up");
        int downIndex = FindCompositePartBindingIndex(action, "down");
        int leftIndex = FindCompositePartBindingIndex(action, "left");
        int rightIndex = FindCompositePartBindingIndex(action, "right");

        // 기존 override 제거 (핵심)
        action.RemoveBindingOverride(upIndex);
        action.RemoveBindingOverride(downIndex);
        action.RemoveBindingOverride(leftIndex);
        action.RemoveBindingOverride(rightIndex);

        // 다시 적용
        if (upIndex >= 0)
            action.ApplyBindingOverride(upIndex, up);

        if (downIndex >= 0)
            action.ApplyBindingOverride(downIndex, down);

        if (leftIndex >= 0)
            action.ApplyBindingOverride(leftIndex, left);

        if (rightIndex >= 0)
            action.ApplyBindingOverride(rightIndex, right);
    }

    private static int FindCompositePartBindingIndex(InputAction action, string partName)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (binding.isPartOfComposite &&
                string.Equals(binding.name, partName, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    // --------------------------------------------
    // UI 버튼용
    // --------------------------------------------

    /// <summary>
    /// 저장값만 초기화한다. 즉시 반영은 하지 않는다.
    /// </summary>
    public void ForceResetAllNow()
    {
        ResetPlayerSettings();

        if (soundPlayer != null)
            soundPlayer.PlayUIClickSound();
    }

    /// <summary>
    /// 저장값 초기화 + 런타임 적용 + UI 갱신
    /// 설정 초기화 버튼은 이 함수를 연결하는 것을 추천.
    /// </summary>
    public void ResetAndApply()
    {
        ResetPlayerSettings();
        ApplyRuntimeSettings();
        RefreshSettingUI();

        if (soundPlayer != null)
            soundPlayer.PlayUIClickSound();
    }

    private void ApplyRuntimeSettings()
    {
        var loader = MainUI_KeyMapLoader.GetOrFind();
        if (loader != null)
        {
            if (currentPlayerInput == null)
                currentPlayerInput = FindFirstObjectByType<PlayerInput>();

            if (currentPlayerInput != null)
            {
                loader.ApplyToPlayerInput(currentPlayerInput, true);
            }
        }

        string lang = ES3.Load<string>(KEY_LANGUAGE, defaultValue: "en-US");

        if (settingGeneral != null)
            settingGeneral.ApplyLanguageByCode(lang);
    }

    private void RefreshSettingUI()
    {
        foreach (var item in FindObjectsByType<MainUI_SettingKeyMapping>(FindObjectsSortMode.None))
        {
            item.ForceRefreshLabel();
        }

        if (settingVideo != null)
            settingVideo.SettingEnabled();

        if (settingAudio != null)
            settingAudio.SettingEnabled();

        if (settingGeneral != null)
            settingGeneral.SettingEnabled();
    }

    private static string DetectSystemLanguageCode()
    {
        try
        {
            string cultureName = CultureInfo.CurrentCulture.Name;
            if (!string.IsNullOrEmpty(cultureName))
                return cultureName;
        }
        catch
        {
        }

        return ConvertSystemLanguage(Application.systemLanguage);
    }

    private static string MapToSupportedLanguage(string systemLang)
    {
        if (string.IsNullOrEmpty(systemLang))
            return "en-US";

        for (int i = 0; i < MainUI_SettingGeneral.languageCodes.Length; i++)
        {
            if (string.Equals(MainUI_SettingGeneral.languageCodes[i], systemLang, System.StringComparison.OrdinalIgnoreCase))
                return MainUI_SettingGeneral.languageCodes[i];
        }

        string shortCode = systemLang;
        int dashIndex = systemLang.IndexOf('-');
        if (dashIndex >= 0)
            shortCode = systemLang.Substring(0, dashIndex);

        for (int i = 0; i < MainUI_SettingGeneral.languageCodes.Length; i++)
        {
            if (MainUI_SettingGeneral.languageCodes[i].StartsWith(shortCode + "-", System.StringComparison.OrdinalIgnoreCase))
                return MainUI_SettingGeneral.languageCodes[i];
        }

        switch (Application.systemLanguage)
        {
            case SystemLanguage.German: return "de-DE";
            case SystemLanguage.English: return "en-US";
            case SystemLanguage.Spanish: return "es-ES";
            case SystemLanguage.French: return "fr-FR";
            case SystemLanguage.Japanese: return "ja-JP";
            case SystemLanguage.Korean: return "ko-KR";
            case SystemLanguage.Polish: return "pl-PL";
            case SystemLanguage.Portuguese: return "pt-BR";
            case SystemLanguage.Russian: return "ru-RU";
            case SystemLanguage.Turkish: return "tr-TR";
            case SystemLanguage.ChineseSimplified: return "zh-CN";
            default: return "en-US";
        }
    }

    private static string ConvertSystemLanguage(SystemLanguage lang)
    {
        switch (lang)
        {
            case SystemLanguage.Korean: return "ko-KR";
            case SystemLanguage.English: return "en-US";
            case SystemLanguage.Japanese: return "ja-JP";
            case SystemLanguage.ChineseSimplified: return "zh-CN";
            case SystemLanguage.German: return "de-DE";
            case SystemLanguage.Spanish: return "es-ES";
            case SystemLanguage.French: return "fr-FR";
            case SystemLanguage.Russian: return "ru-RU";
            case SystemLanguage.Turkish: return "tr-TR";
            case SystemLanguage.Polish: return "pl-PL";
            case SystemLanguage.Portuguese: return "pt-BR";
            default: return "en-US";
        }
    }
}