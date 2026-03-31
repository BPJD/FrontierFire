using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CameraMovingSystem : MonoBehaviour
{
    Transform tr;
    public PlayerLookMouse lookMouse;

    [SerializeField] CinemachinePositionComposer positionComposer;

    public bool isCamRangeUp { get; private set; } = false;
    public bool isSniAiming = false;
    [SerializeField] PlayerMove playerMoveSystem;

    [Header("Boss Direction")]
    [SerializeField] private bool isBossDirectionPlaying = false;
    private Transform bossTarget;

    [Header("Camera Shake")]
    [SerializeField] private bool useUnscaledTimeForShake = false;

    private Coroutine shakeCoroutine;
    private Vector3 shakeOffset = Vector3.zero;

    float _range = 5f;

    Vector3 bossDirectionOffset = Vector3.zero;

    public float cameraRange
    {
        get { return _range; }
        set { _range = value; }
    }

    void Start()
    {
        tr = GetComponent<Transform>();

        if (lookMouse == null || playerMoveSystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);
            lookMouse = player.GetComponent<PlayerLookMouse>();
            playerMoveSystem = player.GetComponent<PlayerMove>();
        }
    }

    void FixedUpdate()
    {
        if (lookMouse == null)
            return;

        Vector3 targetPos;

        // 1. 보스 연출 중이면 보스 위치를 따라감
        if (isBossDirectionPlaying)
        {
            targetPos = GetBossCameraPosition();
        }
        else
        {
            // 2. 평소에는 기존 플레이어/조준 로직 수행
            Vector3 playerPos = lookMouse.playerTr.position + Vector3.up;

            if (playerMoveSystem.isAiming)
            {
                float camRangeRevision = 1f;
                if (isSniAiming && isCamRangeUp)
                {
                    camRangeRevision = 3f;
                }

                Vector3 direction = (lookMouse.targetPos - playerPos).normalized;
                float actualDistance = Vector3.Distance(playerPos, lookMouse.targetPos);
                float distanceToMove = Mathf.Min(cameraRange * camRangeRevision, actualDistance);

                targetPos = playerPos + (direction * distanceToMove);

                Debug.DrawLine(playerPos, targetPos, lookMouse.isAimClose ? Color.red : Color.green);
            }
            else
            {
                targetPos = playerPos;
            }
        }

        // 최종 위치에 흔들림 오프셋 추가
        tr.position = targetPos + shakeOffset;
    }

    private Vector3 GetBossCameraPosition()
    {
        if (bossTarget == null)
            return tr.position;

        return bossTarget.position + bossDirectionOffset;
    }

    /// <summary>
    /// 보스 연출 시작
    /// </summary>
    public void StartBossDirection(Transform bossTr, Vector3 offset)
    {
        if (bossTr == null)
            return;

        isBossDirectionPlaying = true;
        bossTarget = bossTr;
        bossDirectionOffset = offset;
    }

    /// <summary>
    /// 보스 연출 종료
    /// </summary>
    public void EndBossDirection()
    {
        isBossDirectionPlaying = false;
        bossTarget = null;
        bossDirectionOffset = Vector3.zero;
    }

    public void CamControlSet(bool _isCamRangeUp)
    {
        isCamRangeUp = _isCamRangeUp;
    }

    public void CamSpeedSet(bool _isAimKeyDown)
    {
        Vector3 speed = _isAimKeyDown ? new Vector3(2f, 1f, 0f) : new Vector3(1f, 0.5f, 0f);
        positionComposer.Damping = speed;

        playerMoveSystem.isSprintable = !_isAimKeyDown;
    }

    /// <summary>
    /// 화면 흔들림 시작
    /// strength: 흔들림 강도
    /// duration: 지속 시간
    /// </summary>
    public void PlayCameraShake(float strength, float duration)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(CameraShakeRoutine(strength, duration));
    }

    private IEnumerator CameraShakeRoutine(float strength, float duration)
    {
        shakeOffset = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float delta = useUnscaledTimeForShake ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += delta;

            // 시간이 지날수록 약하게 줄어드는 감쇠
            float t = 1f - Mathf.Clamp01(elapsed / duration);
            float currentStrength = strength * t;

            float offsetX = Random.Range(-1f, 1f) * currentStrength;
            float offsetY = Random.Range(-1f, 1f) * currentStrength;

            shakeOffset = new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeCoroutine = null;
    }

    /// <summary>
    /// 흔들림 강제 종료
    /// </summary>
    public void StopCameraShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        shakeOffset = Vector3.zero;
    }
}