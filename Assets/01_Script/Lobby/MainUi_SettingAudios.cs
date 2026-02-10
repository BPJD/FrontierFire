using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.Heat;
using UnityEngine.Audio;

public class MainUi_SettingAudios : MonoBehaviour
{
    MainUI_SettingManager settingManager;

    int[] selectedVolumes = { 70, 70, 70, 70, 70 };
    int[] savedVolumes = { 70, 70, 70, 70, 70 };

    [SerializeField] bool[] selectedisMute = { false, false, false, false, false };
    bool[] savedisMute = { false, false, false, false, false };

    [SerializeField] Slider[] volumeSliders = new Slider[5];
    [SerializeField] SwitchManager[] muteSwitches = new SwitchManager[5];

    [Header("AudioMixer")]
    [SerializeField] AudioMixer audioMixer;

    // AudioMixer에서 Expose 해둔 파라미터 이름들 (필수)
    // 예) Master_Volume, BGM_Volume, SFX_Volume, Ambient_Volume, UI_Volume
    [SerializeField]
    string[] volumeParamNames = new string[5]
    {
        "Vol_Master",
        "Vol_Music",
        "Vol_SFX",
        "Vol_Ambient",
        "Vol_UI"
    };

    const float MUTE_DB = -80f;

    public void SettingEnabled()
    {
        if (settingManager == null)
            settingManager = GetComponentInParent<MainUI_SettingManager>();

        savedVolumes[0] = ES3.Load("Setting_Volume_Master", 70);
        savedVolumes[1] = ES3.Load("Setting_Volume_BGM", 70);
        savedVolumes[2] = ES3.Load("Setting_Volume_SFX", 70);
        savedVolumes[3] = ES3.Load("Setting_Volume_Ambient", 70);
        savedVolumes[4] = ES3.Load("Setting_Volume_UI", 70);

        savedisMute[0] = ES3.Load("Setting_isMute_Master", false);
        savedisMute[1] = ES3.Load("Setting_isMute_BGM", false);
        savedisMute[2] = ES3.Load("Setting_isMute_SFX", false);
        savedisMute[3] = ES3.Load("Setting_isMute_Ambient", false);
        savedisMute[4] = ES3.Load("Setting_isMute_UI", false);

        for (int i = 0; i < 5; i++)
        {
            selectedVolumes[i] = savedVolumes[i];
            selectedisMute[i] = savedisMute[i];

            volumeSliders[i].value = savedVolumes[i];
            volumeSliders[i].SetValueWithoutNotify(selectedVolumes[i]);
            muteSwitches[i].isOn = selectedisMute[i]; // SwitchManager는 보통 이걸로 충분
            muteSwitches[i].UpdateUI();
        }

        // 저장값을 실제 믹서에 반영
        ApplyToMixerAllSelected();

        RecalculateChanged();
    }

    public void VolumeChanged(int code)
    {
        selectedVolumes[code] = (int)volumeSliders[code].value;

        ApplyToMixer(code, selectedVolumes[code], selectedisMute[code]);
        RecalculateChanged();
    }

    public void MuteChanged(int code)
    {
        selectedisMute[code] = muteSwitches[code].isOn;

        ApplyToMixer(code, selectedVolumes[code], selectedisMute[code]);
        RecalculateChanged();
    }

    void ApplyToMixerAllSelected()
    {
        for (int i = 0; i < 5; i++)
            ApplyToMixer(i, selectedVolumes[i], selectedisMute[i]);
    }

    void ApplyToMixer(int code, int volume, bool isMute)
    {
        if (audioMixer == null) return;
        if (code < 0 || code >= volumeParamNames.Length) return;

        float db = VolumeToDb(volume, isMute);
        audioMixer.SetFloat(volumeParamNames[code], db);
    }

    void RecalculateChanged()
    {
        if (settingManager == null) return;

        bool changed = false;
        for (int i = 0; i < 5; i++)
        {
            if (selectedVolumes[i] != savedVolumes[i] || selectedisMute[i] != savedisMute[i])
            {
                changed = true;
                break;
            }
        }
        settingManager.isSettingChanged = changed;

        ApplyToMixerAllSelected();
    }

    public void ApplyOptions()
    {
        // 저장
        ES3.Save("Setting_Volume_Master", selectedVolumes[0]);
        ES3.Save("Setting_Volume_BGM", selectedVolumes[1]);
        ES3.Save("Setting_Volume_SFX", selectedVolumes[2]);
        ES3.Save("Setting_Volume_Ambient", selectedVolumes[3]);
        ES3.Save("Setting_Volume_UI", selectedVolumes[4]);

        ES3.Save("Setting_isMute_Master", selectedisMute[0]);
        ES3.Save("Setting_isMute_BGM", selectedisMute[1]);
        ES3.Save("Setting_isMute_SFX", selectedisMute[2]);
        ES3.Save("Setting_isMute_Ambient", selectedisMute[3]);
        ES3.Save("Setting_isMute_UI", selectedisMute[4]);

        // 기준값 갱신
        for (int i = 0; i < 5; i++)
        {
            savedVolumes[i] = selectedVolumes[i];
            savedisMute[i] = selectedisMute[i];
        }

        RecalculateChanged();
    }

    float VolumeToDb(int volume, bool isMute)
    {
        if (isMute || volume <= 0) return MUTE_DB;
        return Mathf.Log10(volume / 100f) * 20f; // 100 -> 0dB
    }


}
