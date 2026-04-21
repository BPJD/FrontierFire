using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Action names (InputActions 에셋의 Action 이름과 반드시 동일해야 함)
    const string A_Interact = "Interact";
    const string A_Aiming = "Aiming";
    const string A_SniperAiming = "SniperAiming";
    const string A_Move = "Move";
    const string A_Jump = "Jump";
    const string A_DownJump = "DownJump";
    const string A_Sprint = "Sprint";
    const string A_Attack = "Attack";
    const string A_Reload = "Reload";

    const string A_WeaponA = "WeaponA";
    const string A_WeaponB = "WeaponB";
    const string A_WeaponC = "WeaponC";
    const string A_NextWeapon = "NextWeapon";
    const string A_PrevWeapon = "PrevWeapon";

    const string A_HideWeaponInfo = "HideWeaponInfo";
    const string A_Dash = "Dash";
    const string A_Menu = "Menu";

    [SerializeField] string actionMapName = "Player";

    // ─────────────────────────────────────────────
    // Refs
    GameObject module;
    PlayerInteract interacter;
    CameraMovingSystem camMoveSystem;
    PlayerWeapon weaponCur;
    PlayerMove playerMove;
    PlayerWeaponController playerWeaponCon;
    ShieldManager abilityShield;
    PlayerDashManager dashManager;
    UI_Paused uiPaused;

    public TerrainDownPlatform downPlatform { get; set; }

    int selectedWeapon = 2;
    public bool isInfoHide { get; private set; } = false;
    public static string infoHideData = "IsWeaponInfoHide";

    public bool isSprintToggle = true;
    public bool IsInputLocked { get; private set; } = false;
    Vector2 moveInputCached = Vector2.zero;

    // ─────────────────────────────────────────────
    // Input
    PlayerInput playerInput;
    InputActionMap map;

    InputAction actInteract, actAiming, actSniperAiming, actMove, actJump, actDownJump,
                actSprint, actAttack, actReload, actWeaponA, actWeaponB, actWeaponC,
                actNextWeapon, actPrevWeapon, actHideWeaponInfo, actDash, actMenu;

    bool _bound = false;

    // ─────────────────────────────────────────────

    public GameObject playerModelObj;
    public GameObject playerWeaponObj;

    bool isPlayerDead = false;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        ControllerReset();
        CacheActions(); // 여기서 액션 캐시
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // PlayerInput이 Awake 이후에 actions를 갱신하는 케이스 대비
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (map == null) CacheActions();

        BindActions(true);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        BindActions(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ControllerReset();

        // 씬 전환/재생성 상황에서 actions/map이 바뀌는 케이스 대비
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        CacheActions();

        // 이미 바인딩된 상태로 씬이 바뀌면(드물지만) 안전하게 재바인딩
        if (isActiveAndEnabled)
        {
            BindActions(false);
            BindActions(true);
        }
    }

    void ControllerReset()
    {
        module = GameObject.FindGameObjectWithTag("Module");
        uiPaused = module != null ? module.GetComponentInChildren<UI_Paused>() : null;
        interacter = GetComponentInChildren<PlayerInteract>();
        camMoveSystem = module != null ? module.GetComponentInChildren<CameraMovingSystem>() : null;
        playerMove = GetComponent<PlayerMove>();
        playerWeaponCon = GetComponent<PlayerWeaponController>();
        abilityShield = GetComponentInChildren<ShieldManager>();
        dashManager = GetComponentInChildren<PlayerDashManager>();

        isInfoHide = ES3.KeyExists(infoHideData) ? ES3.Load<bool>(infoHideData) : false;
    }

    // ─────────────────────────────────────────────
    // Cache
    void CacheActions()
    {
        map = null;

        if (playerInput == null || playerInput.actions == null)
            return;

        if (!string.IsNullOrEmpty(actionMapName))
            map = playerInput.actions.FindActionMap(actionMapName, throwIfNotFound: false);

        if (map != null)
        {
            actInteract = map.FindAction(A_Interact, false);
            actAiming = map.FindAction(A_Aiming, false);
            actSniperAiming = map.FindAction(A_SniperAiming, false);
            actMove = map.FindAction(A_Move, false);
            actJump = map.FindAction(A_Jump, false);
            actDownJump = map.FindAction(A_DownJump, false);
            actSprint = map.FindAction(A_Sprint, false);
            actAttack = map.FindAction(A_Attack, false);
            actReload = map.FindAction(A_Reload, false);

            actWeaponA = map.FindAction(A_WeaponA, false);
            actWeaponB = map.FindAction(A_WeaponB, false);
            actWeaponC = map.FindAction(A_WeaponC, false);
            actNextWeapon = map.FindAction(A_NextWeapon, false);
            actPrevWeapon = map.FindAction(A_PrevWeapon, false);

            actHideWeaponInfo = map.FindAction(A_HideWeaponInfo, false);
            actDash = map.FindAction(A_Dash, false);
            actMenu = map.FindAction(A_Menu, false);
        }
        else
        {
            var actions = playerInput.actions;

            actInteract = actions.FindAction(A_Interact, false);
            actAiming = actions.FindAction(A_Aiming, false);
            actSniperAiming = actions.FindAction(A_SniperAiming, false);
            actMove = actions.FindAction(A_Move, false);
            actJump = actions.FindAction(A_Jump, false);
            actDownJump = actions.FindAction(A_DownJump, false);
            actSprint = actions.FindAction(A_Sprint, false);
            actAttack = actions.FindAction(A_Attack, false);
            actReload = actions.FindAction(A_Reload, false);

            actWeaponA = actions.FindAction(A_WeaponA, false);
            actWeaponB = actions.FindAction(A_WeaponB, false);
            actWeaponC = actions.FindAction(A_WeaponC, false);
            actNextWeapon = actions.FindAction(A_NextWeapon, false);
            actPrevWeapon = actions.FindAction(A_PrevWeapon, false);

            actHideWeaponInfo = actions.FindAction(A_HideWeaponInfo, false);
            actDash = actions.FindAction(A_Dash, false);
            actMenu = actions.FindAction(A_Menu, false);
        }
    }

    void WarnIfNull(InputAction a, string name)
    {
        if (a == null)
            Debug.LogWarning($"[PlayerInputController] Action not found: {name} (check InputActions name / map)");
    }

    // ─────────────────────────────────────────────
    // Bind / Unbind
    void BindActions(bool bind)
    {
        if (_bound == bind) return;
        _bound = bind;

        // 액션들이 없으면(캐시 실패) 종료
        if (playerInput == null || playerInput.actions == null) return;

        // 각 액션 구독
        // 단발(performed), 버튼 Down/Up(started/canceled), 값형(performed/canceled)
        Link(actInteract, performed: OnInteract, bind: bind);

        Link(actAiming, started: OnAimingDown, canceled: OnAimingUp, bind: bind);
        Link(actSniperAiming, started: OnSniperAimingDown, canceled: OnSniperAimingUp, bind: bind);

        Link(actMove, performed: OnMove, canceled: OnMove, bind: bind);

        Link(actJump, performed: OnJump, bind: bind);
        Link(actDownJump, performed: OnDownJump, bind: bind);

        Link(actSprint, started: OnSprintDown, canceled: OnSprintUp, bind: bind);

        Link(actAttack, started: OnAttackDown, canceled: OnAttackUp, bind: bind);

        Link(actReload, performed: OnReload, bind: bind);

        Link(actWeaponA, performed: OnWeaponA, bind: bind);
        Link(actWeaponB, performed: OnWeaponB, bind: bind);
        Link(actWeaponC, performed: OnWeaponC, bind: bind);
        Link(actNextWeapon, performed: OnNextWeapon, bind: bind);
        Link(actPrevWeapon, performed: OnPrevWeapon, bind: bind);

        Link(actHideWeaponInfo, performed: OnHideWeaponInfo, bind: bind);
        Link(actDash, performed: OnDash, bind: bind);
        Link(actMenu, performed: OnMenu, bind: bind);

        // Enable 처리:
        // - PlayerInput이 액션맵 enable/disable을 관리하는 경우가 많아서,
        //   여기서 개별 action.Enable()을 강제하지 않는 게 안전하다.
        // - 만약 입력이 안 들어오면, 아래 EnableAllActions()를 켜면 됨.
        //EnableAllActionsIfNeeded();
    }

    void Link(
        InputAction a,
        System.Action<InputAction.CallbackContext> performed = null,
        System.Action<InputAction.CallbackContext> started = null,
        System.Action<InputAction.CallbackContext> canceled = null,
        bool bind = true)
    {
        if (a == null) return;

        if (bind)
        {
            if (performed != null) a.performed += performed;
            if (started != null) a.started += started;
            if (canceled != null) a.canceled += canceled;
        }
        else
        {
            if (performed != null) a.performed -= performed;
            if (started != null) a.started -= started;
            if (canceled != null) a.canceled -= canceled;
        }
    }

    // 필요 시 사용 (입력이 0개 들어올 때만 켜서 원인 확인용)
    void EnableAllActionsIfNeeded()
    {
        // actionMap이 있으면 map만 enable하는 게 정석
        if (map != null && !map.enabled) map.Enable();
        else if (playerInput != null && playerInput.currentActionMap != null && !playerInput.currentActionMap.enabled)
            playerInput.currentActionMap.Enable();
    }

    // ─────────────────────────────────────────────
    // Callbacks (기존 로직 유지)

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (IsInputLocked) return;
        interacter?.Interacted();
    }

    void OnAimingDown(InputAction.CallbackContext ctx)
    {
        camMoveSystem?.CamSpeedSet(true);
        if (playerMove != null) playerMove.isAiming = true;
        abilityShield?.ShieldActivate(true);

        if (weaponCur != null && weaponCur.laserScope != null)
            weaponCur.ScopeControl(true);
    }

    void OnAimingUp(InputAction.CallbackContext ctx)
    {
        camMoveSystem?.CamSpeedSet(false);
        if (playerMove != null) playerMove.isAiming = false;
        abilityShield?.ShieldActivate(false);

        if (weaponCur != null && weaponCur.laserScope != null)
            weaponCur.ScopeControl(false);
    }

    void OnSniperAimingDown(InputAction.CallbackContext ctx)
    {
        if (camMoveSystem != null && camMoveSystem.isCamRangeUp)
            camMoveSystem.isSniAiming = true;
    }

    void OnSniperAimingUp(InputAction.CallbackContext ctx)
    {
        if (camMoveSystem != null)
            camMoveSystem.isSniAiming = false;
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        if (playerMove == null) return;

        if (IsInputLocked)
        {
            moveInputCached = Vector2.zero;
            playerMove.MoveRequested(Vector2.zero);
            return;
        }

        Vector2 v = ctx.ReadValue<Vector2>();

        moveInputCached = v;
        playerMove.MoveRequested(v);

        if (isSprintToggle && v.sqrMagnitude <= 0.01f)
            playerMove.SprintEndRequested();
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        if (IsInputLocked) return;
        playerMove?.JumpRequested();
    }

    void OnDownJump(InputAction.CallbackContext ctx)
    {
        downPlatform?.DownJumpRequested();
    }

    void OnSprintDown(InputAction.CallbackContext ctx)
    {
        playerMove?.SprintStartRequested();
    }

    void OnSprintUp(InputAction.CallbackContext ctx)
    {
        if (!isSprintToggle)
            playerMove?.SprintEndRequested();
    }

    void OnAttackDown(InputAction.CallbackContext ctx)
    {
        if (IsInputLocked) return;
        weaponCur?.Input_Shoot(true);
    }

    void OnAttackUp(InputAction.CallbackContext ctx)
    {
        if (IsInputLocked) return;
        weaponCur?.Input_Shoot(false);
    }

    void OnReload(InputAction.CallbackContext ctx)
    {
        if (playerWeaponCon == null || interacter == null) return;

        if (playerWeaponCon.playersWeapons[selectedWeapon] != null && interacter.SelectedObj == null)
        {
            StartCoroutine(ReloadInvoke());
        }
            
    }

    void OnWeaponA(InputAction.CallbackContext ctx) => ChangeWeapon(0);
    void OnWeaponB(InputAction.CallbackContext ctx) => ChangeWeapon(1);
    void OnWeaponC(InputAction.CallbackContext ctx) => ChangeWeapon(2);

    void OnNextWeapon(InputAction.CallbackContext ctx)
    {
        int next = selectedWeapon + 1;
        if (next > 2) next = 0;
        ChangeWeapon(next);
    }

    void OnPrevWeapon(InputAction.CallbackContext ctx)
    {
        int prev = selectedWeapon - 1;
        if (prev < 0) prev = 2;
        ChangeWeapon(prev);
    }

    void ChangeWeapon(int idx)
    {
        selectedWeapon = idx;
        playerWeaponCon?.WeaponChangeRequested(selectedWeapon);
    }

    void OnHideWeaponInfo(InputAction.CallbackContext ctx)
    {
        isInfoHide = !isInfoHide;
        ES3.Save(infoHideData, isInfoHide);
    }

    void OnDash(InputAction.CallbackContext ctx)
    {
        if (IsInputLocked) return;

        Vector3 dashDir = new Vector3(moveInputCached.x, moveInputCached.y, 0f);

        // 상하 없는 횡스크롤 회피라면 이렇게:
        // Vector3 dashDir = new Vector3(moveInputCached.x, 0f, 0f);

        dashManager?.DashActive(dashDir);
    }

    void OnMenu(InputAction.CallbackContext ctx)
    {
        if (IsInputLocked) return;
        uiPaused?.PauseUIActive();
    }

    // ─────────────────────────────────────────────
    public void Requested_WeaponReady(PlayerWeapon pWeapon)
    {
        weaponCur = pWeapon;
    }

    public void PlayerDead()
    {
        playerInput.SwitchCurrentActionMap("UI");
        // 사망 시 UI 액션맵으로 전환
    }

    IEnumerator ReloadInvoke()
    {
        yield return new WaitForSeconds(0.05f);
        weaponCur?.Input_Reload();
    }


    public void SetInputLock(bool locked)
    {
        IsInputLocked = locked;

        if (locked)
        {
            ForceStopAllPlayerActions();
        }
    }

    void ForceStopAllPlayerActions()
    {
        if (playerMove != null)
        {
            playerMove.MoveRequested(Vector2.zero);
            playerMove.SprintEndRequested();
            playerMove.isAiming = false;
        }

        camMoveSystem?.CamSpeedSet(false);
        abilityShield?.ShieldActivate(false);

        if (camMoveSystem != null)
            camMoveSystem.isSniAiming = false;

        weaponCur?.Input_Shoot(false);

        if (weaponCur != null && weaponCur.laserScope != null)
            weaponCur.ScopeControl(false);
    }

    public void RefreshActionBindings()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        BindActions(false);
        CacheActions();
        BindActions(true);

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap(actionMapName);
    }
}
