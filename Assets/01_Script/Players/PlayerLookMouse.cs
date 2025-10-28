using UnityEngine;
using UnityEngine.InputSystem; // ← 추가
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

    // ↓ 추가: 입력 소스 구분(마우스 vs 패드 오른스틱)
    bool useMouse = true;
    Vector2 rightStick; // 패드 조준 벡터

    [SerializeField] float forwardAimDistance = 8f; // 앞을 바라볼 때 고정 조준 거리
    [SerializeField] float rayDistance = 50f;    // 기존 50f 그대로 쓰려면 이 값 참조

    [SerializeField] Camera cam; // 인스펙터로 연결 가능(선택)

    void Awake()
    {
        TryBindCamera();
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMove = GetComponent<PlayerMove>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;     // 새 씬 로드되면 다시 바인딩
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m) => TryBindCamera();
    void OnActiveSceneChanged(Scene oldS, Scene newS) => TryBindCamera();

    void TryBindCamera()
    {
        // 이미 유효하면 종료
        if (cam && cam.isActiveAndEnabled) return;

        // 1) 가장 먼저 MainCamera 태그 시도
        var main = Camera.main;
        if (main && main.isActiveAndEnabled) { cam = main; return; }

        // 2) 활성 카메라 스캔 (신규 API 사용)
        Camera candidate = null;

        var cams = Object.FindObjectsByType<Camera>(
            FindObjectsInactive.Include,         // 비활성까지 포함
            FindObjectsSortMode.None             // 정렬 불필요 → 더 빠름
        );
        // 우선순위: (MainCamera && 활성) > (활성) > (첫 번째 아무 카메라)
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


    // Input System - Look 액션(C# Events)
    public void OnLook(InputValue value)
    {
        // 패드 오른스틱 or 마우스 포인터가 Vector2로 들어옴
        Vector2 v = value.Get<Vector2>();

        // (예: 기존 코드에서 쓰던 rightStick/useMouse 갱신)
        rightStick = v;

        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        // 현재 컨트롤 스킴으로 마우스/패드 분기 (간단 버전)
        useMouse = !string.IsNullOrEmpty(playerInput.currentControlScheme)
                   && playerInput.currentControlScheme.Contains("Keyboard");
    }

    void Update()
    {
        if (useMouse)
        {
            if (!cam) { TryBindCamera(); if (!cam) return; } // ← 추가: 널 가드

            // ── 마우스: 기존 로직 그대로 ──
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);  // ← Camera.main -> cam
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayDistance, raycastLayerMask))
            {
                _isAimClose = hit.collider.CompareTag("RayBlock");

                // 2D 환경: z 고정(원래 코드 유지)
                hitPos = new Vector3(hit.point.x, hit.point.y, 0f);

                HandleCharacterRotation(targetPos);
            }
        }
        else
        {
            AimWithGamepad_UseCameraRay();
        }
    }

    // 캐릭터 좌/우 방향 회전 처리 (원본 유지)
    void HandleCharacterRotation(Vector3 _targetPos)
    {
        if (meshTr.position.x <= _targetPos.x)
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

        //패드 조작 중이 아닐 때: 각도 0도로 앞을 바라보게
        if (rightStick.sqrMagnitude < deadzone * deadzone)
        {
            Vector3 dir =
                (playerMove != null && playerMove.isLookingRight) ? Vector3.right : Vector3.left;

            Vector3 forwardPoint = padAimingPinTr.position + dir * forwardAimDistance;

            _isAimClose = false; // 막힘 아님으로 처리
            hitPos = new Vector3(forwardPoint.x, padAimingPinTr.position.y, 0f); // 수평(각도 0)
            HandleCharacterRotation(targetPos);
            return;
        }

        // ── 패드 조작 중: 카메라 레이 방식 그대로 ──
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float radius = Mathf.Min(Screen.width, Screen.height) * 0.35f;
        Vector2 screenPos = screenCenter + rightStick.normalized * radius;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, raycastLayerMask))
        {
            _isAimClose = hit.collider.CompareTag("RayBlock");
            hitPos = new Vector3(hit.point.x, hit.point.y, 0f);
            HandleCharacterRotation(targetPos);
        }
    }

}
