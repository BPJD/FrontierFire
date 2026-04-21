using Michsky.UI.Heat;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MainUI_SettingGeneral : MonoBehaviour
{
    MainUI_SettingManager settingManager;
    [SerializeField] Localize_FontManager fontManager;

    public static readonly string[] languageCodes = { "de-DE", "en-US", "es-ES", "fr-FR", "ja-JP", "ko-KR", "pl-PL", "pt-BR", "ru-RU", "tr-TR", "zh-CN" };
    [SerializeField] private LocalizationManager locManager;

    [SerializeField] HorizontalSelector languageSelector;
    int savedLanguageIndex = 0;
    public int selectedLanguageIndex = 0;

    UI_InputDeviceDetector inputDetector;
    [SerializeField] GameObject firstSelect;

    UI_SoundPlayer uiSoundPlayer;

    private void OnEnable()
    {
        if (inputDetector == null)
            inputDetector = GameObject.FindGameObjectWithTag("Module").GetComponent<UI_InputDeviceDetector>();

        SettingEnabled();

        uiSoundPlayer = GetComponentInParent<UI_SoundPlayer>();

        if (inputDetector.currentInputType == UI_InputDeviceDetector.InputType.Gamepad)
            EventSystem.current.SetSelectedGameObject(firstSelect);
    }

    public void SettingEnabled()
    {
        if (settingManager == null)
            settingManager = GetComponentInParent<MainUI_SettingManager>();

        string savedLanguageCode = ES3.Load<string>("Setting_Language", defaultValue: "en-US");
        savedLanguageIndex = GetLanguageIndex(savedLanguageCode);
        selectedLanguageIndex = savedLanguageIndex;

        locManager.SetLanguage(languageCodes[selectedLanguageIndex]);

        languageSelector.index = selectedLanguageIndex;
        languageSelector.defaultIndex = selectedLanguageIndex;
        languageSelector.UpdateUI();

        if (fontManager != null)
            fontManager.UpdateFontForLanguage(languageCodes[selectedLanguageIndex]);
    }


    public void OnChangeLanguage()
    {
        selectedLanguageIndex = languageSelector.index;

        string selectedLanguageCode = languageCodes[selectedLanguageIndex];

        locManager.SetLanguage(selectedLanguageCode);
        LocalizationManager.SetLanguageWithoutNotify(selectedLanguageCode);
        ES3.Save("Setting_Language", selectedLanguageCode);

        languageSelector.defaultIndex = selectedLanguageIndex;
        languageSelector.UpdateUI();

        if (fontManager != null)
            fontManager.UpdateFontForLanguage(selectedLanguageCode);

        StartCoroutine(ResetToolTipUI());

        if(uiSoundPlayer != null)
        {
            uiSoundPlayer.PlayUIClickSound();
        }
    }

    IEnumerator ResetToolTipUI()
    {
        GameObject _canvas = GameObject.FindGameObjectWithTag("ItemUICanvas");
        if(_canvas != null)
        {
            _canvas.SetActive(false);
            yield return null; // Wait for one frame
            _canvas.SetActive(true);
        }
    }

    private int GetLanguageIndex(string languageCode)
    {
        for (int i = 0; i < languageCodes.Length; i++)
        {
            if (languageCodes[i] == languageCode)
                return i;
        }

        return 1; // en-US
    }

    public void ApplyLanguageByCode(string languageCode)
    {
        int index = GetLanguageIndex(languageCode);

        selectedLanguageIndex = index;
        savedLanguageIndex = index;

        locManager.SetLanguage(languageCode);
        LocalizationManager.SetLanguageWithoutNotify(languageCode);

        languageSelector.index = index;
        languageSelector.defaultIndex = index;
        languageSelector.UpdateUI();

        if (fontManager != null)
            fontManager.UpdateFontForLanguage(languageCode);

        if (isActiveAndEnabled)
        {
            StartCoroutine(ResetToolTipUI());
        }
        
    }


}
