using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerLookMouse : MonoBehaviour
{
    [SerializeField] Transform meshTr;          // 캐릭터 본체
    [SerializeField] Transform padAimingPinTr;
    public Transform weaponTr;                  // 무기
    public LayerMask raycastLayerMask;
    private Vector3 hitPos = Vector3.zero;

    private bool _isAimClose = false;
    public bool isAimClose { get { return _isAimClose; } }

    public Transform playerTr { get { return meshTr; } }
    public Vector3 targetPos { get { return hitPos; } }

    PlayerMove playerMove;
    PlayerInput playerInput;

    // 입력 소스 구분
    bool useMouse = true;

    // 입력 값 분리 저장 (중요)
    Vector2 mouseScreenPos;  // 픽셀 좌표(0~w, 0~h)
    Vector2 rightStick;      // (-1~1) 방향 벡터

    [SerializeField] float forwardAimDistance = 8f;
    [SerializeField] float rayDistance = 50f;

    AbilityController abilityController;
    bool lastLookingRight;

    [SerializeField] Camera cam; // 인스펙터로 연결 가능(선택)

    // C# Events 구독용
    InputAction lookAction;

    void Awake()
    {
        TryBindCamera();
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMove = GetComponent<PlayerMove>();
        abilityController = GetComponentInChildren<AbilityController>();

        CacheLookAction(); // actions 준비되면 캐싱
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        // C# Events 구독
        BindLookAction(true);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        // C# Events 구독 해제
        BindLookAction(false);
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m) => TryBindCamera();
    void OnActiveSceneChanged(Scene oldS, Scene newS) => TryBindCamera();





    // 일시정지로 인한 에임 차단

    private bool _inputBlocked;

    // 외부(PauseManager/UI)에서 호출
    public void SetInputBlocked(bool blocked)
    {
        if (_inputBlocked == blocked) return;
        _inputBlocked = blocked;

        // 1) Update 차단은 _inputBlocked로 처리
        // 2) 입력 이벤트 자체도 차단(선택: 둘 중 하나만 해도 되지만 둘 다가 가장 안전)
        BindLookAction(!blocked);

        // 3) 정지 순간 마지막 입력 잔류값 제거(스틱 유지/마우스 마지막 좌표 잔상 방지)
        rightStick = Vector2.zero;
        mouseScreenPos = Vector2.zero;
    }

    // 일시정지로 인한 에임 차단




    void TryBindCamera()
    {
        if (cam && cam.isActiveAndEnabled) return;

        var main = Camera.main;
        if (main && main.isActiveAndEnabled) { cam = main; return; }

        Camera candidate = null;
        var cams = Object.FindObjectsByType<Camera>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var c in cams)
        {
            if (!c) continue;
            if (c.CompareTag("MainCamera") && c.isActiveAndEnabled) { candidate = c; break; }
            if (candidate == null && c.isActiveAndEnabled) candidate = c;
        }
        if (candidate == null && cams.Length > 0) candidate = cams[0];

        cam = candidate;
        if (cam == null)
            Debug.LogWarning("[PlayerLookMouse] No Camera found yet. Will retry next frame.");
    }

    void CacheLookAction()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (playerInput == null || playerInput.actions == null) return;

        // 액션 이름이 "Look"라고 가정 (네 기존 OnLook 연결 기준)
        lookAction = playerInput.actions.FindAction("Look", throwIfNotFound: false);
        if (lookAction == null)
            Debug.LogWarning("[PlayerLookMouse] Look action not found. Check action name in InputActions.");
    }

    void BindLookAction(bool bind)
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (lookAction == null) CacheLookAction();
        if (lookAction == null) return;

        if (bind)
        {
            lookAction.performed += OnLookAction;
            lookAction.canceled += OnLookAction; // 스틱 놓을 때 0 들어오는 처리
        }
        else
        {
            lookAction.performed -= OnLookAction;
            lookAction.canceled -= OnLookAction;
        }
    }

    // C# Events 콜백 (핵심)
    void OnLookAction(InputAction.CallbackContext ctx)
    {
        Vector2 v = ctx.ReadValue<Vector2>();

        // 어떤 디바이스 입력인지로 분기 (스킴 문자열에 의존하지 않음)
        var device = ctx.control?.device;

        if (device is Mouse || device is Pointer)
        {
            useMouse = true;

            // Look 바인딩이 "Point"면 v가 screen position(픽셀)로 들어옴
            mouseScreenPos = v;

            // 만약 바인딩이 Delta였을 때 대비(좌표가 비정상 범위면) -> fallback
            if (mouseScreenPos.x < 0 || mouseScreenPos.y < 0 ||
                mouseScreenPos.x > Screen.width * 2f || mouseScreenPos.y > Screen.height * 2f)
            {
                mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
            }
        }
        else if (device is Gamepad)
        {
            useMouse = false;
            rightStick = v; // (-1~1)
        }
        else
        {
            // 알 수 없는 디바이스면 현재 스킴으로 fallback
            string scheme = playerInput != null ? playerInput.currentControlScheme : "";
            bool schemeIsMouse = !string.IsNullOrEmpty(scheme) && (scheme.Contains("Keyboard") || scheme.Contains("Mouse"));
            useMouse = schemeIsMouse;

            if (useMouse) mouseScreenPos = v;
            else rightStick = v;
        }
    }

    void Update()
    {
        if (_inputBlocked) return;
        if (!cam) { TryBindCamera(); if (!cam) return; }

        if (useMouse)
        {
            // 마우스 조준
            Vector2 sp = mouseScreenPos;

            // mouseScreenPos가 아직 한 번도 안 들어왔으면 fallback
            if (sp == Vector2.zero)
                sp = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;

            Ray ray = cam.ScreenPointToRay(sp);
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, raycastLayerMask))
            {
                _isAimClose = hit.collider.CompareTag("RayBlock");
                hitPos = new Vector3(hit.point.x, hit.point.y, 0f);
                HandleCharacterRotation(targetPos);
            }
        }
        else
        {
            AimWithGamepad_UseCameraRay();
        }
    }

    void HandleCharacterRotation(Vector3 _targetPos)
    {
        bool lookingRightNow = meshTr.position.x <= _targetPos.x;

        if (lookingRightNow != lastLookingRight)
        {
            lastLookingRight = lookingRightNow;
            abilityController.PlayerTurned(!lookingRightNow);
        }

        if (lookingRightNow)
        {
            meshTr.LookAt(meshTr.position + Vector3.right);
            playerMove.isLookingRight = true;
        }
        else
        {
            meshTr.LookAt(meshTr.position + Vector3.left);
            playerMove.isLookingRight = false;
        }
    }

    void AimWithGamepad_UseCameraRay()
    {
        const float deadzone = 0.2f;

        if (rightStick.sqrMagnitude < deadzone * deadzone)
        {
            Vector3 dir = (playerMove != null && playerMove.isLookingRight) ? Vector3.right : Vector3.left;
            Vector3 forwardPoint = padAimingPinTr.position + dir * forwardAimDistance;

            _isAimClose = false;
            hitPos = new Vector3(forwardPoint.x, padAimingPinTr.position.y, 0f);
            HandleCharacterRotation(targetPos);
            return;
        }

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float radius = Mathf.Min(Screen.width, Screen.height) * 0.35f;
        Vector2 screenPos = screenCenter + rightStick.normalized * radius;

        // Camera.main 쓰지 말고 cam 사용(일관성)
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, raycastLayerMask))
        {
            _isAimClose = hit.collider.CompareTag("RayBlock");
            hitPos = new Vector3(hit.point.x, hit.point.y, 0f);
            HandleCharacterRotation(targetPos);
        }
    }
}
