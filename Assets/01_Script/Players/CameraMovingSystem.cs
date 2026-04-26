using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CameraMovingSystem : MonoBehaviour
{
    private Transform tr;

    public PlayerLookMouse lookMouse;

    [SerializeField] private CinemachinePositionComposer positionComposer;

    public bool isCamRangeUp { get; private set; } = false;
    public bool isSniAiming = false;

    [SerializeField] private PlayerMove playerMoveSystem;

    [Header("Stage Camera Move")]
    [SerializeField] private float stageMoveSmoothTime = 0.45f;
    [SerializeField] private float stageMoveEndDistance = 0.05f;
    [SerializeField] private bool useUnscaledTimeForStageMove = false;

    private bool isStageCameraMoving = false;
    private Vector3 stageMoveVelocity = Vector3.zero;

    [Header("Boss Direction")]
    [SerializeField] private bool isBossDirectionPlaying = false;
    private Transform bossTarget;
    private Vector3 bossDirectionOffset = Vector3.zero;

    [Header("Camera Shake")]
    [SerializeField] private bool useUnscaledTimeForShake = false;

    private Coroutine shakeCoroutine;
    private Vector3 shakeOffset = Vector3.zero;

    private float _range = 5f;

    public float cameraRange
    {
        get { return _range; }
        set { _range = value; }
    }

    private void Awake()
    {
        tr = transform;
    }

    private void Start()
    {
        if (lookMouse == null || playerMoveSystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(Data_Strings.playerTag);

            if (player != null)
            {
                if (lookMouse == null)
                    lookMouse = player.GetComponent<PlayerLookMouse>();

                if (playerMoveSystem == null)
                    playerMoveSystem = player.GetComponent<PlayerMove>();
            }
        }
    }

    private void FixedUpdate()
    {
        if (lookMouse == null)
            return;

        Vector3 targetPos = GetTargetPosition();
        Vector3 finalTargetPos = targetPos + shakeOffset;

        if (isStageCameraMoving)
        {
            MoveCameraTargetSmoothly(finalTargetPos);
        }
        else
        {
            tr.position = finalTargetPos;
        }
    }

    private Vector3 GetTargetPosition()
    {
        if (isBossDirectionPlaying)
            return GetBossCameraPosition();

        Vector3 playerPos = lookMouse.playerTr.position + Vector3.up;
        playerPos.z = 0f;

        if (playerMoveSystem != null && playerMoveSystem.isAiming)
        {
            float camRangeRevision = 1f;

            if (isSniAiming && isCamRangeUp)
                camRangeRevision = 3f;

            Vector3 direction = (lookMouse.targetPos - playerPos).normalized;
            float actualDistance = Vector3.Distance(playerPos, lookMouse.targetPos);
            float distanceToMove = Mathf.Min(cameraRange * camRangeRevision, actualDistance);

            Vector3 targetPos = playerPos + direction * distanceToMove;
            targetPos.z = 0f;

            Debug.DrawLine(playerPos, targetPos, lookMouse.isAimClose ? Color.red : Color.green);

            return targetPos;
        }

        return playerPos;
    }

    private void MoveCameraTargetSmoothly(Vector3 finalTargetPos)
    {
        float deltaTime = useUnscaledTimeForStageMove
            ? Time.fixedUnscaledDeltaTime
            : Time.fixedDeltaTime;

        tr.position = Vector3.SmoothDamp(
            tr.position,
            finalTargetPos,
            ref stageMoveVelocity,
            stageMoveSmoothTime,
            Mathf.Infinity,
            deltaTime
        );

        if ((tr.position - finalTargetPos).sqrMagnitude <= stageMoveEndDistance * stageMoveEndDistance)
        {
            tr.position = finalTargetPos;
            stageMoveVelocity = Vector3.zero;
            isStageCameraMoving = false;
        }
    }

    public void StartStageCameraMove()
    {
        isStageCameraMoving = true;
        stageMoveVelocity = Vector3.zero;
    }

    public void StopStageCameraMove(bool snapToTarget = false)
    {
        isStageCameraMoving = false;
        stageMoveVelocity = Vector3.zero;

        if (snapToTarget && lookMouse != null)
        {
            tr.position = GetTargetPosition() + shakeOffset;
        }
    }

    private Vector3 GetBossCameraPosition()
    {
        if (bossTarget == null)
            return tr.position;

        Vector3 targetPos = bossTarget.position + bossDirectionOffset;
        targetPos.z = 0f;

        return targetPos;
    }

    public void StartBossDirection(Transform bossTr, Vector3 offset)
    {
        if (bossTr == null)
            return;

        isBossDirectionPlaying = true;
        bossTarget = bossTr;
        bossDirectionOffset = offset;
    }

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
        if (positionComposer != null)
        {
            Vector3 speed = _isAimKeyDown
                ? new Vector3(2f, 1f, 0f)
                : new Vector3(1f, 0.5f, 0f);

            positionComposer.Damping = speed;
        }

        if (playerMoveSystem != null)
            playerMoveSystem.isSprintable = !_isAimKeyDown;
    }

    public void PlayCameraShake(float strength, float duration)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(CameraShakeRoutine(strength, duration));
    }

    private IEnumerator CameraShakeRoutine(float strength, float duration)
    {
        shakeOffset = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float delta = useUnscaledTimeForShake
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            elapsed += delta;

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