using UnityEngine;
using UnityEngine.InputSystem;

public class MainUI_KeyMapLoader : MonoBehaviour
{
    public static MainUI_KeyMapLoader Instance { get; private set; }

    [Header("Input (Master Actions)")]
    [SerializeField] private InputActionAsset actions;
    public InputActionAsset Actions => actions;

    [Header("Easy Save 3")]
    [SerializeField] private string es3FileName = "keymap.es3";
    [SerializeField] private string es3Key = "input.bindingOverridesJson";

    [Header("Lifecycle")]
    [Tooltip("메뉴→게임 씬 전환에서도 유지하고 싶으면 체크")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        if (actions == null)
            return;

        Load();
    }

    public void Save()
    {
        if (actions == null)
            return;

        string json = actions.SaveBindingOverridesAsJson();
        ES3.Save(es3Key, json, es3FileName);
    }

    public void Load()
    {
        if (actions == null)
            return;

        string json = ES3.Load<string>(es3Key, es3FileName, defaultValue: "");

        if (string.IsNullOrEmpty(json))
            return;

        actions.LoadBindingOverridesFromJson(json);
    }

    public void ResetToDefault()
    {
        if (actions == null)
            return;

        actions.RemoveAllBindingOverrides();
        ES3.DeleteKey(es3Key, es3FileName);
    }

    public void ApplyTo(InputActionAsset targetActions)
    {
        if (actions == null || targetActions == null)
            return;

        string json = actions.SaveBindingOverridesAsJson();

        targetActions.RemoveAllBindingOverrides();

        if (!string.IsNullOrEmpty(json))
            targetActions.LoadBindingOverridesFromJson(json);
    }

    public void ApplyToPlayerInput(PlayerInput playerInput, bool reEnableActions = true)
    {
        if (playerInput == null || playerInput.actions == null)
            return;

        ApplyTo(playerInput.actions);

        if (reEnableActions)
        {
            playerInput.actions.Disable();
            playerInput.actions.Enable();
        }
    }

    public void ResetToDefaultAndApply(PlayerInput playerInput = null, bool reEnableActions = true)
    {
        ResetToDefault();

        if (playerInput != null && playerInput.actions != null)
        {
            playerInput.actions.RemoveAllBindingOverrides();

            if (reEnableActions)
            {
                playerInput.actions.Disable();
                playerInput.actions.Enable();
            }
        }
    }

    public static MainUI_KeyMapLoader GetOrFind()
    {
        if (Instance != null) return Instance;

        Instance = FindFirstObjectByType<MainUI_KeyMapLoader>();
        if (Instance != null) return Instance;

        return null;
    }

    public void ApplyAllToRuntime()
    {
        var playerInput = FindFirstObjectByType<PlayerInput>();
        var inputController = FindFirstObjectByType<PlayerInputController>();

        if (playerInput != null)
            ApplyToPlayerInput(playerInput, true);

        if (inputController != null)
        {
            inputController.RefreshActionBindings();
            inputController.SetInputLock(false);
        }
    }
}