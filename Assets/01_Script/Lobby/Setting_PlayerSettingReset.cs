using UnityEngine;
using UnityEngine.InputSystem;

public class Setting_PlayerSettingReset : MonoBehaviour
{
    [SerializeField] MainUI_SettingVideos settingVideo;
    [SerializeField] MainUi_SettingAudios settingAudio;

    // -----------------------
    // ES3 Files
    // -----------------------
    //private const string FILE_SETTINGS = "settings.es3"; // 오디오/비디오/일반
    private const string FILE_KEYMAP = "keymap.es3";   // 조작(키맵)

    // -----------------------
    // ES3 Keys (General)
    // -----------------------
    private const string KEY_FIRST_RUN_DONE = "Player_FirstRunDone";
    private const string KEY_PLAY_COUNT = "Player_PlayCount";

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
    // ES3 Keys (Keymap)
    // -----------------------
    private const string KEY_INPUT_OVERRIDES_JSON = "input.bindingOverridesJson";

    private void Awake()
    {
        // 1) PlayCount는 "통계" 용도로만 증가시키고,
        // 2) 실제 초기화는 FirstRunDone 플래그로 1회만 수행하는 구조가 안전함.
        bool firstRunDone = ES3.Load(KEY_FIRST_RUN_DONE, false);

        if (!firstRunDone)
        {
            ResetPlayerSettings();
            ES3.Save(KEY_FIRST_RUN_DONE, true);
        }

        int playCount = ES3.Load(KEY_PLAY_COUNT, 0);
        playCount++;
        ES3.Save(KEY_PLAY_COUNT, playCount);

        //Debug.Log(playCount + "th play.");
    }

    /// <summary>
    /// 플레이어 설정 초기화(오디오/비디오/조작)
    /// - 오디오/비디오: settings.es3에 저장
    /// - 조작(키맵): keymap.es3에서 override 삭제 + 런타임 적용 제거(가능하면)
    /// </summary>
    public void ResetPlayerSettings()
    {
        ResetAudioDefaults();
        ResetVideoDefaults();
        ResetControlDefaults();
    }

    private static void ResetAudioDefaults()
    {
        // 볼륨(0~100 기준이라고 가정)
        ES3.Save(KEY_VOL_MASTER, 70);
        ES3.Save(KEY_VOL_BGM, 70);
        ES3.Save(KEY_VOL_SFX, 70);
        ES3.Save(KEY_VOL_AMBIENT, 70);
        ES3.Save(KEY_VOL_UI, 70);

        // 뮤트
        ES3.Save(KEY_MUTE_MASTER, false);
        ES3.Save(KEY_MUTE_BGM, false);
        ES3.Save(KEY_MUTE_SFX, false);
        ES3.Save(KEY_MUTE_AMBIENT, false);
        ES3.Save(KEY_MUTE_UI, false);
    }

    private static void ResetVideoDefaults()
    {
        // 네 기존 기본값 유지
        ES3.Save(KEY_RESOLUTION_INDEX, 5);
        ES3.Save(KEY_FPS_LIMIT, 60);
        ES3.Save(KEY_IS_FPS_LIMIT, false);
        ES3.Save(KEY_VSYNC, false);
        ES3.Save(KEY_SCREEN_MODE, 1);   // (int)1 유지
        ES3.Save(KEY_QUALITY_INDEX, 2);
    }

    private static void ResetControlDefaults()
    {
        // 1) 저장된 키맵 override 삭제 (다음 실행/로드 시 기본키)
        ES3.DeleteKey(KEY_INPUT_OVERRIDES_JSON, FILE_KEYMAP);

        // 2) 런타임에 KeyMapLoader가 존재하면 즉시 기본키로 되돌림
        //    (메뉴 씬에서 실행되는 경우라면 Loader가 있을 확률이 높음)
        var loader = MainUI_KeyMapLoader.GetOrFind();
        if (loader != null && loader.Actions != null)
        {
            loader.Actions.RemoveAllBindingOverrides();
            // loader 쪽 파일명이 keymap.es3인지 확인 필요:
            // - 아래가 가장 깔끔: MainUI_KeyMapLoader가 FILE_KEYMAP/KEY_INPUT_OVERRIDES_JSON를 쓰도록 통일
            // - 현재 Loader가 settings.es3를 쓰고 있다면, Loader도 FILE_KEYMAP로 바꿔주는 게 맞음.
            // 여기서는 "저장키 삭제 + 런타임 override 제거"까지만 보장.
        }

        Debug.Log("[SettingsReset] Control(Keymap) reset complete.");
    }

    // --------------------------------------------
    // (선택) UI에서 '설정 초기화' 버튼을 눌렀을 때 쓰는 API
    // --------------------------------------------

    public void ForceResetAllNow()
    {
        ResetPlayerSettings();
        // 초기화는 여러 번 해도 무방하지만, first-run 플래그는 유지해도 됨.
        // 필요하면 여기서 KEY_FIRST_RUN_DONE을 false로 바꾸는 기능도 추가 가능.
    }

    public void ResetAndApply()
    {
        ResetPlayerSettings();   // 1) ES3 값 초기화

        // 2) 런타임 시스템에 적용
        ApplyRuntimeSettings();

        // 3) UI 갱신
        RefreshSettingUI();
    }

    private void ApplyRuntimeSettings()
    {
        // 오디오/비디오 적용은 네 프로젝트에 이미 있는 SettingManager 쪽 메서드가 있으면 그걸 호출하는 게 정답.
        // 예시:
        // FindFirstObjectByType<AudioSettingApplier>()?.ApplyFromES3();
        // FindFirstObjectByType<VideoSettingApplier>()?.ApplyFromES3();

        // 키맵은 여기서 확실히 처리 가능
        var loader = MainUI_KeyMapLoader.GetOrFind();
        if (loader != null)
        {
            loader.Actions.RemoveAllBindingOverrides(); // 런타임 즉시 기본값
                                                        // 저장키도 이미 삭제했으니, 필요하면 Save()는 안 해도 됨(기본값 상태가 유지되니까)
        }
    }

    private void RefreshSettingUI()
    {
        // 설정 UI 전체를 관리하는 매니저가 있다면 거기서 한 번에 리프레시하는 게 좋다.
        // 예시:
        // FindFirstObjectByType<MainUI_SettingManager>()?.RefreshAll();

        // 키맵 UI(각 버튼) 갱신은 “리바인드 아이템들”을 찾아서 Refresh 호출
        foreach (var item in FindObjectsByType<MainUI_SettingKeyMapping>(FindObjectsSortMode.None))
            item.ForceRefreshLabel();


        settingVideo.SettingEnabled();
        settingAudio.SettingEnabled();
    }
}
