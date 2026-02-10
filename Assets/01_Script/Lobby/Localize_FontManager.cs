using UnityEngine;
using Michsky.UI.Heat;
using TMPro;

public class Localize_FontManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;

    [SerializeField] TMP_FontAsset ch_Light;
    [SerializeField] TMP_FontAsset ch_Regular;
    [SerializeField] TMP_FontAsset ch_Medium;
    [SerializeField] TMP_FontAsset ch_SemiBold;
    [SerializeField] TMP_FontAsset ch_Bold;

    [SerializeField] TMP_FontAsset jp_Light;
    [SerializeField] TMP_FontAsset jp_Regular;
    [SerializeField] TMP_FontAsset jp_Medium;
    [SerializeField] TMP_FontAsset jp_SemiBold;
    [SerializeField] TMP_FontAsset jp_Bold;

    [SerializeField] TMP_FontAsset pretendard_Light;
    [SerializeField] TMP_FontAsset pretendard_Regular;
    [SerializeField] TMP_FontAsset pretendard_Medium;
    [SerializeField] TMP_FontAsset pretendard_SemiBold;
    [SerializeField] TMP_FontAsset pretendard_Bold;

    public string languageCur { get; set; } = "";


    public void UpdateFontForLanguage(string languageCode)
    {
        TMP_FontAsset lightFont;
        TMP_FontAsset regularFont;
        TMP_FontAsset mediumFont;
        TMP_FontAsset semiBoldFont;
        TMP_FontAsset boldFont;
        switch (languageCode)
        {
            case "zh-CN":
                lightFont = ch_Light;
                regularFont = ch_Regular;
                mediumFont = ch_Medium;
                semiBoldFont = ch_SemiBold;
                boldFont = ch_Bold;
                break;
            case "ja-JP":
                lightFont = jp_Light;
                regularFont = jp_Regular;
                mediumFont = jp_Medium;
                semiBoldFont = jp_SemiBold;
                boldFont = jp_Bold;
                break;
            default:
                lightFont = pretendard_Light;
                regularFont = pretendard_Regular;
                mediumFont = pretendard_Medium;
                semiBoldFont = pretendard_SemiBold;
                boldFont = pretendard_Bold;
                break;
        }
        uiManager.fontLight = lightFont;
        uiManager.fontRegular = regularFont;
        uiManager.fontMedium = mediumFont;
        uiManager.fontSemiBold = semiBoldFont;
        uiManager.fontBold = boldFont;

        languageCur = languageCode;
    }
}
