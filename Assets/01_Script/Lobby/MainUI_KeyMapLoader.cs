using UnityEngine;
using UnityEngine.InputSystem;

public class MainUI_KeyMapLoader : MonoBehaviour
{
    public static MainUI_KeyMapLoader Instance { get; private set; }

    [Header("Input (Master Actions)")]
    [SerializeField] private InputActionAsset actions;
    public InputActionAsset Actions => actions;

    [Header("Easy Save 3")]
    [SerializeField] private string es3FileName = "settings.es3";
    [SerializeField] private string es3Key = "input.bindingOverridesJson";

    [Header("Lifecycle")]
    [Tooltip("메뉴→게임 씬 전환에서도 유지하고 싶으면 체크")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        if (actions == null)
        {
            Debug.LogError("[KeyMapLoader] InputActionAsset(actions) is null.");
            return;
        }

        Load(); // 시작 시 로드 적용
    }

    /// <summary>현재 바인딩 오버라이드를 ES3에 저장</summary>
    public void Save()
    {
        if (actions == null)
        {
            Debug.LogError("[KeyMapLoader] Save failed. actions is null.");
            return;
        }

        string json = actions.SaveBindingOverridesAsJson();
        ES3.Save(es3Key, json, es3FileName);
        Debug.Log("[KeyMapLoader] Saved binding overrides.");
    }

    /// <summary>ES3에서 바인딩 오버라이드를 불러와 적용</summary>
    public void Load()
    {
        if (actions == null)
        {
            Debug.LogError("[KeyMapLoader] Load failed. actions is null.");
            return;
        }

        string json = ES3.Load<string>(es3Key, es3FileName, defaultValue: "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("[KeyMapLoader] No saved overrides found. Using defaults.");
            return;
        }

        actions.LoadBindingOverridesFromJson(json);
        Debug.Log("[KeyMapLoader] Loaded & applied binding overrides.");
    }

    /// <summary>사용자가 '기본값으로 초기화' 눌렀을 때</summary>
    public void ResetToDefault()
    {
        if (actions == null)
        {
            Debug.LogError("[KeyMapLoader] ResetToDefault failed. actions is null.");
            return;
        }

        actions.RemoveAllBindingOverrides();
        ES3.DeleteKey(es3Key, es3FileName);
        Debug.Log("[KeyMapLoader] Reset to default and deleted saved overrides.");
    }

    /// <summary>
    /// (선택) PlayerInput이 별도 actions 인스턴스를 쓰는 구조일 때,
    /// KeyMapLoader의 현재 override를 타겟 actions에 복사 적용.
    /// </summary>
    public void ApplyTo(InputActionAsset targetActions)
    {
        if (actions == null || targetActions == null) return;

        string json = actions.SaveBindingOverridesAsJson();
        if (!string.IsNullOrEmpty(json))
            targetActions.LoadBindingOverridesFromJson(json);
    }

    /// <summary>
    /// Instance가 씬에 없을 수 있는 상황(메뉴 씬에서 누락 등)에 대비해,
    /// 호출 시점에 안전하게 찾아오거나 생성할 수 있는 헬퍼.
    /// </summary>
    public static MainUI_KeyMapLoader GetOrFind()
    {
        if (Instance != null) return Instance;

        Instance = FindFirstObjectByType<MainUI_KeyMapLoader>();
        if (Instance != null) return Instance;

        Debug.LogError("[KeyMapLoader] Instance not found in scene. Please add MainUI_KeyMapLoader to a persistent GameObject.");
        return null;
    }
}
