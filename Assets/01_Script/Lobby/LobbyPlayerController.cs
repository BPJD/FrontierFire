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

    [SerializeField] GameObject inGameUI;

    public bool isPlayerInLobby { get; set; } = false;

    bool isPlayerWeaponEquiped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inGameUI.SetActive(false);
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
                PlayerInputEnableCheck();
            }
        }
    }

    void PlayerInputEnableCheck()
    {
        playerInput.enabled = isPlayerInLobby;
        playerInputComp.enabled = isPlayerInLobby;
        playerInput.playerModelObj.SetActive(isPlayerInLobby);
        playerInput.playerWeaponObj.SetActive(isPlayerInLobby);
        playerAnimator.SetTrigger("NoneDraw");
    }


    public void ButtonClick_Play()
    {
        cam_Main.SetActive(false);
        cam_Player.SetActive(true);

        StartCoroutine(PlayerInTheLobby());

    }

    public void ButtonClick_ToMain()
    {
        cam_Main.SetActive(true);
        cam_Player.SetActive(false);

        StartCoroutine(PlayerOutTheLobby());

    }

    IEnumerator PlayerInTheLobby()
    {
        PlayerWeaponUnEquip();

        yield return new WaitForSeconds(2f);

        isPlayerInLobby = true;
        PlayerInputEnableCheck();

        inLobbyIdleEft.Stop(true);
        inLobbyEft.Play(true);
    }


    IEnumerator PlayerOutTheLobby()
    {
        yield return null;
        isPlayerInLobby = false;
        PlayerInputEnableCheck();

        player.transform.position = startPoint.position;

        inLobbyIdleEft.Play(true);
        
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
        inGameUI.SetActive(false);
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
        inGameUI.SetActive(true);
    }




}
