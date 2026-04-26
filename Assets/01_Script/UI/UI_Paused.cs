using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UI_Paused : MonoBehaviour
{
    [SerializeField] GameObject panelPauseMain;
    [SerializeField] GameObject panelPause;
    [SerializeField] GameObject panelSetting;
    [SerializeField] GameObject panelConfirmToMain;
    [SerializeField] GameObject resumeButton;

    PlayerInput playerInput;
    PlayerLookMouse playerLookMouse;
    PlayerInputController playerInputController;
    UI_InputDeviceDetector inputDetector;
    [SerializeField] Direction_SceneChanger sceneChanger;

    [SerializeField] bool isLobbyScene = false;

    UI_SoundPlayer uiSoundPlayer;


    const string PLAYER_MAP = "Player";
    const string UI_MAP = "UI";

    private void Start()
    {
        inputDetector = GetComponentInParent<UI_InputDeviceDetector>();
        uiSoundPlayer = GetComponent<UI_SoundPlayer>();
        CheckPlayerComponent();
    }

    void CheckPlayerComponent()
    {
        if (playerInput == null || playerLookMouse == null || playerInputController == null)
        {
            GameObject _player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);

            playerInput = _player.GetComponent<PlayerInput>();
            playerLookMouse = _player.GetComponent<PlayerLookMouse>();
            playerInputController = _player.GetComponent<PlayerInputController>();

        }
    }


    public void PauseUIActive()
    {
        CheckPlayerComponent();

        if (!panelSetting.activeSelf)
        {
            panelPause.SetActive(!panelPause.activeSelf);
        }

        if (panelPause.activeSelf)
        {
            Time.timeScale = 0f;
            //playerInput.actions.FindActionMap(Data_Strings.playerTag).Disable();
            playerInput.SwitchCurrentActionMap(UI_MAP);
            playerLookMouse.SetInputBlocked(true);
            playerInputController.SetInputLock(true);

        }
        else
        {
            Time.timeScale = 1f;
            //playerInput.actions.FindActionMap(Data_Strings.playerTag).Enable();
            playerInput.SwitchCurrentActionMap(PLAYER_MAP);
            playerLookMouse.SetInputBlocked(false);
            playerInputController.SetInputLock(false);

        }

        switch (inputDetector.currentInputType)
        {
            case UI_InputDeviceDetector.InputType.Gamepad:
                EventSystem.current.SetSelectedGameObject(resumeButton);
                break;
            case UI_InputDeviceDetector.InputType.KeyboardMouse:
                MousePointerLock(panelPause.activeSelf);
                break;
            default:
                break;
        }



    }

    void MousePointerLock(bool isPause)
    {
        if (isPause)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            //Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;
        }
    }


    public void ButtonResume()
    {
        PauseUIActive();
        uiSoundPlayer.PlayUIClickSound();
    }

    public void ButtonSetting()
    {
        SettingUIActive(true);
        uiSoundPlayer.PlayUIClickSound();
    }

    public void ButtonToMain()
    {
        panelConfirmToMain.SetActive(true);
        uiSoundPlayer.PlayUIClickSound();
    }


    void SettingUIActive(bool isActive)
    {
        panelSetting.SetActive(isActive);
        panelPauseMain.SetActive(!isActive);
        uiSoundPlayer.PlayUINotiOn();
    }

    public void BackToMenuConfirm(bool isBack)
    {
        if (isBack)
        {
            sceneChanger.ToMainMenu();
            PauseUIActive();
        }
        else
        {
            panelConfirmToMain.SetActive(false);
        }

        uiSoundPlayer.PlayUINotiOff();
    }


}
