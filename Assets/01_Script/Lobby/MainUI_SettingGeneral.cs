using Michsky.UI.Heat;
using UnityEditor.Search;
using UnityEngine;

public class MainUI_SettingGeneral : MonoBehaviour
{
    MainUI_SettingManager settingManager;
    [SerializeField] Localize_FontManager fontManager;

    string[] languageCodes = { "de-DE", "en-US", "es-ES", "fr-FR", "ja-JP", "ko-KR", "pl-PL", "pt-BR", "ru-RU", "tr-TR", "zh-CN" };
    [SerializeField] private LocalizationManager locManager;

    [SerializeField] HorizontalSelector languageSelector;
    int savedLanguageIndex = 0;
    public int selectedLanguageIndex = 0;


    public void SettingEnabled()
    {
        if (settingManager == null)
        {
            settingManager = GetComponentInParent<MainUI_SettingManager>();
        }

        savedLanguageIndex = ES3.Load<int>("Setting_Language");
        selectedLanguageIndex = savedLanguageIndex;
        locManager.SetLanguage(languageCodes[selectedLanguageIndex]);
        languageSelector.index = selectedLanguageIndex;

        if(fontManager != null)
        {
            fontManager.UpdateFontForLanguage(languageCodes[selectedLanguageIndex]);
        }

    }


    public void OnChangeLanguage()
    {
        selectedLanguageIndex = languageSelector.index;

        // Set language
        locManager.SetLanguage(languageCodes[selectedLanguageIndex]);
        ES3.Save<int>("Setting_Language", selectedLanguageIndex);

        // Set language without notifying
        // No reference required for this call as it's static
        LocalizationManager.SetLanguageWithoutNotify(languageCodes[selectedLanguageIndex]);

        if (fontManager != null)
        {
            fontManager.UpdateFontForLanguage(languageCodes[selectedLanguageIndex]);
        }
    }
}
