using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class LobbyPlayerController : MonoBehaviour
{

    GameObject player;

    PlayerInputController playerInput;
    PlayerWeaponController playerWeapon;
    PlayerInput playerInputComp;
    Animator playerAnimator;

    [SerializeField] GameObject cam_Main;
    [SerializeField] GameObject cam_Player;

    [SerializeField] ParticleSystem inLobbyEft;
    [SerializeField] ParticleSystem inLobbyIdleEft;

    [SerializeField] Transform startPoint;

    [SerializeField] GameObject[] inGameUI;

    [SerializeField] GameObject canvas_Main;

    public bool isPlayerInLobby { get; set; } = false;

    bool isPlayerWeaponEquiped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InGameUIActive(false);

        if(ES3.Load<bool>("isStartInLobby", false))
        {
            ES3.Save<bool>("isStartInLobby", false);
            ButtonClick_Play();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
            if(player != null)
            {
                playerInput = player.GetComponent<PlayerInputController>();
                playerWeapon = player.GetComponent<PlayerWeaponController>();
                playerInputComp = player.GetComponent<PlayerInput>();
                playerAnimator = player.GetComponentInChildren<Animator>();
                ApplyLobbyState(isPlayerInLobby);
            }
        }
    }

    void InGameUIActive(bool isActive)
    {
        for (int i = 0; i < inGameUI.Length; i++)
        {
            inGameUI[i].SetActive(isActive);
        }
    }

    void ApplyLobbyState(bool inLobby)
    {
        // 1) 입력 컴포넌트는 끄지 말 것
        if (playerInputComp != null)
            playerInputComp.enabled = true;

        if (playerInput != null)
            playerInput.enabled = true;

        // 2) 액션맵 전환 (여기서 UI/Player를 확정)
        if (playerInputComp != null)
        {
            // 메인(=UI 화면)
            if (!inLobby) playerInputComp.SwitchCurrentActionMap("UI");
            // 로비(=플레이어 조작 구간이면 Player)
            else playerInputComp.SwitchCurrentActionMap("Player");
        }

        // 3) 캐릭터 표시/무기 표시만 상태에 맞게
        if (playerInput != null)
        {
            playerInput.playerModelObj.SetActive(inLobby);
            playerInput.playerWeaponObj.SetActive(inLobby);
        }

        if (playerAnimator != null)
            playerAnimator.SetTrigger("NoneDraw");
    }


    public void ButtonClick_Play()
    {
        int _playCount = ES3.Load<int>(Setting_PlayerSettingReset.KEY_PLAY_COUNT, 1);
        if (_playCount == 1)
        {
            GameObject _changer = GameObject.FindGameObjectWithTag("GameController");
            if (_changer != null)
            {
                _changer.GetComponent<Direction_SceneChanger>().ChangeScene("Scene_LobbyTutorial");
            }
        }
        else
        {
            cam_Main.SetActive(false);
            cam_Player.SetActive(true);
            canvas_Main.SetActive(false);

            StartCoroutine(PlayerInTheLobby());
        }
    }

    public void ButtonClick_ToMain()
    {
        cam_Main.SetActive(true);
        cam_Player.SetActive(false);
        canvas_Main.SetActive(true);

        StartCoroutine(PlayerOutTheLobby());

    }

    IEnumerator PlayerInTheLobby()
    {
        PlayerWeaponUnEquip();

        yield return new WaitForSecondsRealtime(2f);

        isPlayerInLobby = true;
        ApplyLobbyState(isPlayerInLobby);

        inLobbyIdleEft.Stop(true);
        inLobbyEft.Play(true);

        //playerInputComp.SwitchCurrentActionMap("Player");
    }


    IEnumerator PlayerOutTheLobby()
    {
        yield return null;
        isPlayerInLobby = false;
        ApplyLobbyState(isPlayerInLobby);

        player.transform.position = startPoint.position;

        inLobbyIdleEft.Play(true);

        //playerInputComp.SwitchCurrentActionMap("UI");
    }


    public void PlayerWeaponUnEquip()
    {
        if (playerWeapon == null)
            return;

        for(int i = 0; i < playerWeapon.playersWeapons.Length; i++)
        {
            if (playerWeapon.playersWeapons[i] != null)
            {
                Destroy(playerWeapon.playersWeapons[i]);
                playerWeapon.playersWeapons[i] = null;
            }
        }

        playerWeapon.WeaponChangeRequested(0);
        playerAnimator.SetTrigger("NoneDraw");
        isPlayerWeaponEquiped = false;
        playerWeapon.UIRefresh();
        InGameUIActive(false);
    }

    public void PlayerWeaponEquip()
    {
        if (isPlayerWeaponEquiped)
        {
            return;
        }

        playerWeapon.GetWeapon(60004, 300, 6, 0, null, 60);
        playerWeapon.GetWeapon(60000, 9999, 6, 0, null, 60);

        isPlayerWeaponEquiped = true;
        InGameUIActive(true);
    }




}
