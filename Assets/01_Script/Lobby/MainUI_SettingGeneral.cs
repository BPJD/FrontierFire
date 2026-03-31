using Michsky.UI.Heat;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MainUI_SettingGeneral : MonoBehaviour
{
    MainUI_SettingManager settingManager;
    [SerializeField] Localize_FontManager fontManager;

    string[] languageCodes = { "de-DE", "en-US", "es-ES", "fr-FR", "ja-JP", "ko-KR", "pl-PL", "pt-BR", "ru-RU", "tr-TR", "zh-CN" };
    [SerializeField] private LocalizationManager locManager;

    [SerializeField] HorizontalSelector languageSelector;
    int savedLanguageIndex = 0;
    public int selectedLanguageIndex = 0;

    UI_InputDeviceDetector inputDetector;
    [SerializeField] GameObject firstSelect;

    private void OnEnable()
    {
        if (inputDetector == null)
            inputDetector = GameObject.FindGameObjectWithTag("Module").GetComponent<UI_InputDeviceDetector>();

        SettingEnabled();

        if (inputDetector.currentInputType == UI_InputDeviceDetector.InputType.Gamepad)
            EventSystem.current.SetSelectedGameObject(firstSelect);
    }

    public void SettingEnabled()
    {
        if (settingManager == null)
            settingManager = GetComponentInParent<MainUI_SettingManager>();

        savedLanguageIndex = ES3.Load<int>("Setting_Language", 1); // 예: 기본값 en-US
        selectedLanguageIndex = savedLanguageIndex;

        // 실제 언어 적용
        locManager.SetLanguage(languageCodes[selectedLanguageIndex]);

        // Selector 표시 동기화
        languageSelector.index = selectedLanguageIndex;
        languageSelector.defaultIndex = selectedLanguageIndex;
        languageSelector.UpdateUI();

        if (fontManager != null)
            fontManager.UpdateFontForLanguage(languageCodes[selectedLanguageIndex]);
    }


    public void OnChangeLanguage()
    {
        selectedLanguageIndex = languageSelector.index;

        locManager.SetLanguage(languageCodes[selectedLanguageIndex]);
        LocalizationManager.SetLanguageWithoutNotify(languageCodes[selectedLanguageIndex]);
        ES3.Save("Setting_Language", selectedLanguageIndex);

        languageSelector.defaultIndex = selectedLanguageIndex;
        languageSelector.UpdateUI();

        if (fontManager != null)
            fontManager.UpdateFontForLanguage(languageCodes[selectedLanguageIndex]);

        StartCoroutine(ResetToolTipUI());
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


}
