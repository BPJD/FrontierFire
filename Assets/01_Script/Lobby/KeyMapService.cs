using UnityEngine;
using UnityEngine.InputSystem;

public class KeyMapService : MonoBehaviour
{
    public static KeyMapService I { get; private set; }

    [Header("Input Actions (Master)")]
    [SerializeField] private InputActionAsset actions; // 기준이 되는 .inputactions (하나만!)

    [Header("ES3")]
    [SerializeField] private string fileName = "keymap.es3";
    [SerializeField] private string keyName = "input.bindingOverridesJson";

    private bool _loaded;

    public InputActionAsset Actions => actions;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        LoadAndApplyToSelf();
    }

    /// <summary>서비스가 들고있는 actions에 저장된 override를 적용</summary>
    public void LoadAndApplyToSelf()
    {
        string json = ES3.Load<string>(keyName, fileName, "");
        if (!string.IsNullOrEmpty(json))
            actions.LoadBindingOverridesFromJson(json);

        _loaded = true;
        Debug.Log("[KeyMapService] Loaded & applied overrides to service actions.");
    }

    public void Save()
    {
        string json = actions.SaveBindingOverridesAsJson();
        ES3.Save(keyName, json, fileName);
        Debug.Log("[KeyMapService] Saved overrides.");
    }

    public void ResetToDefault()
    {
        actions.RemoveAllBindingOverrides();
        ES3.DeleteKey(keyName, fileName);
        Debug.Log("[KeyMapService] Reset to default.");
    }

    /// <summary>
    /// PlayerInput이 사용하는 actions에 "서비스가 가진 override"를 복사 적용
    /// (PlayerInput이 별도 인스턴스를 쓸 때 필수)
    /// </summary>
    public void ApplyTo(InputActionAsset target)
    {
        if (target == null) return;

        // 서비스의 현재 override를 JSON으로 뽑아서 타겟에 그대로 적용
        string json = actions.SaveBindingOverridesAsJson();
        if (!string.IsNullOrEmpty(json))
            target.LoadBindingOverridesFromJson(json);

        Debug.Log("[KeyMapService] Applied overrides to target actions.");
    }
}
