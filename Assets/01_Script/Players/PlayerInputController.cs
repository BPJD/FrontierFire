using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PlayerInputController : MonoBehaviour
{
    GameObject module;
    PlayerInteract interacter;
    CameraMovingSystem camMoveSystem;
    PlayerWeapon weaponCur;
    PlayerMove playerMove;
    PlayerWeaponController playerWeaponCon;
    public TerrainDownPlatform downPlatform { get; set; }

    bool aimingPressedPrev;
    bool sniAimingPressedPrev;
    bool sprintPressedPrev;
    bool shootPressedPrev;
    int selectedWeapon = 0;
    public bool isInfoHide { get; private set; } = false;
    public static string infoHideData = "IsWeaponInfoHide";

    public bool isSprintToggle = true;
    



    private void Awake()
    {
        ControllerReset();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ControllerReset();
    }

    void ControllerReset()
    {
        module = GameObject.FindGameObjectWithTag("Module");
        interacter = GetComponentInChildren<PlayerInteract>();
        camMoveSystem = module.GetComponentInChildren<CameraMovingSystem>();
        playerMove = GetComponent<PlayerMove>();
        playerWeaponCon = GetComponent<PlayerWeaponController>();
        isInfoHide = ES3.Load<bool>(infoHideData);
    }

    public void OnInteract()
    {
        interacter.Interacted();
    }

    public void OnAiming(InputValue value)
    {
        bool pressed = value.Get<float>() > 0.5f;

        // GetButtonDown: false -> true
        if (pressed && !aimingPressedPrev)
        {
            camMoveSystem.CamSpeedSet(true);
            playerMove.isAiming = true;
        }

        // GetButtonUp: true -> false   ← 여기만 수정
        if (!pressed && aimingPressedPrev)
        {
            camMoveSystem.CamSpeedSet(false);
            playerMove.isAiming = false;
        }

        // GetButton (홀드 중)
        // if (pressed) { ... }

        if (weaponCur.laserScope != null)
        {
            weaponCur.ScopeControl(playerMove.isAiming);
            //weaponCur.laserScope.GetComponent<LineRenderer>().SetPosition(1, Vector3.forward * weaponCur.GetBulletRange());
        }

        aimingPressedPrev = pressed; // ← 이 위치(함수 끝) 맞습니다.
    }

    public void OnSniperAiming(InputValue value)
    {
        bool pressed = value.Get<float>() > 0.5f;

        //GetButtonDown
        if (pressed && !sniAimingPressedPrev && camMoveSystem.isCamRangeUp)
        {
            camMoveSystem.isSniAiming = true;
        }

        //GetButtonUp
        if (!pressed && sniAimingPressedPrev && camMoveSystem.isSniAiming)
        {
            camMoveSystem.isSniAiming = false;
        }

        sniAimingPressedPrev = pressed;
    }

    public void OnMove(InputValue value)
    {
        Vector2 v = value.Get<Vector2>();
        playerMove.MoveRequested(v);

        // 이동 입력이 0이면 스프린트 종료
        if (isSprintToggle && v.sqrMagnitude <= 0.01f)
            playerMove.SprintEndRequested();
    }


    public void OnJump()
    {
        playerMove.JumpRequested();
    }

    public void OnDownJump()
    {
        if(downPlatform != null)
        {
            downPlatform.DownJumpRequested();
        }
    }

    public void OnSprint(InputValue value)
    {
        bool pressed = value.Get<float>() > 0.5f;

        //GetButtonDown
        if (pressed && !sprintPressedPrev)
        {
            playerMove.SprintStartRequested();
        }

        //GetButtonUp
        if (!pressed && sprintPressedPrev)
        {
            if (!isSprintToggle)
            {
                playerMove.SprintEndRequested();
            }
        }
        sprintPressedPrev = pressed;
    }

    public void OnAttack(InputValue value)
    {
        bool pressed = value.Get<float>() > 0.5f;

        //GetButtonDown
        if (pressed && !shootPressedPrev && weaponCur != null)
        {
            weaponCur.Input_Shoot(true);
        }

        //GetButtonUp
        if (!pressed && shootPressedPrev && weaponCur != null)
        {
            weaponCur.Input_Shoot(false);
        }

        shootPressedPrev = pressed;
    }



    public void OnReload()
    {
        if (playerWeaponCon.playersWeapons[selectedWeapon] != null && interacter.SelectedObj == null)
        {
            StartCoroutine(ReloadInvoke());
        }
    }

    void WeaponChanged(int weaponNum)
    {
        playerWeaponCon.WeaponChangeRequested(weaponNum);
    }

    public void OnWeaponA()
    {
        selectedWeapon = 0;
        WeaponChanged(selectedWeapon);
    }

    public void OnWeaponB()
    {
        selectedWeapon = 1;
        WeaponChanged(selectedWeapon);
    }

    public void OnWeaponC()
    {
        selectedWeapon = 2;
        WeaponChanged(selectedWeapon);
    }

    public void OnNextWeapon()
    {
        selectedWeapon++;

        if(selectedWeapon > 2)
        {
            selectedWeapon = 0;
        }

        WeaponChanged(selectedWeapon);
    }

    public void OnPrevWeapon()
    {
        selectedWeapon--;

        if (selectedWeapon < 0)
        {
            selectedWeapon = 2;
        }
        WeaponChanged(selectedWeapon);
    }

    public void Requested_WeaponReady(PlayerWeapon pWeapon) //PlayerWeapon에서 호출됨
    {
        weaponCur = pWeapon;
    }

    public void OnHideWeaponInfo()
    {
        isInfoHide = !isInfoHide;
        Debug.Log("Weapon Info Hide Toggled: " + isInfoHide);

        ES3.Save<bool>(infoHideData, isInfoHide);
    }

    IEnumerator ReloadInvoke()
    {
        yield return new WaitForSeconds(0.05f);
        weaponCur.Input_Reload();
    }
}
