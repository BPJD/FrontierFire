using UnityEngine;

public class BossGiantMove : MonoBehaviour
{
    UnitStatus unitStat;

    [SerializeField] float moveSpeed = 5f;
    Transform tr;
    Transform playerTr;
    Animator aniCon;
    BossGiantAttackControl bossAttackAI;
    BossControlSystem bossState;

    [SerializeField] float distanceToPlayer = 5f;
    float distanceCur;
    public bool isClose { get; private set; } = false;

    string ani_direction = "Direction";

    public bool isMove { get; set; } = false;

    // ▼ 이동 스무딩 관련
    [SerializeField] float moveSmooth = 8f;
    float currentMoveSpeed = 0f;
    float targetMoveSpeed = 0f;

    // ▼ 애니메이터 스무딩
    [SerializeField] float directionSmooth = 10f;
    float currentDirection = 0.5f;
    float targetDirection = 0.5f;
    float directionVel = 0f;


    private void Start()
    {
        tr = transform;
        unitStat = GetComponent<UnitStatus>();
        //moveSpeed = unitStat.moveSpeed;

        playerTr = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).transform;

        distanceToPlayer *= distanceToPlayer;

        aniCon = GetComponent<Animator>();
        bossAttackAI = GetComponent<BossGiantAttackControl>();

        bossState = GetComponent<BossControlSystem>();

        // Animator Behaviour에서 콜백 연결
        var behaviours = aniCon.GetBehaviours<BossGiant_Anicon_Move>();
        foreach (var b in behaviours)
        {
            b.OnStart = OnStart;
        }
    }

    private void FixedUpdate()
    {
        if (!isMove || bossAttackAI.isStun || !bossState.isBossLive) return;

        // ──────────────────────────────
        // 1) 플레이어 거리 체크
        // ──────────────────────────────
        distanceCur = (tr.position - playerTr.position).sqrMagnitude;
        isClose = distanceToPlayer > distanceCur;


        // ──────────────────────────────
        // 2) 방향 결정 + 애니메이션 방향 페이드
        // ──────────────────────────────
        if (!isClose)
        {
            bool isLeft = playerTr.position.x > tr.position.x;

            // 캐릭터가 왼쪽으로 갈지 오른쪽으로 갈지
            targetDirection = isLeft ? 0f : 1f;

            // 이동 속도 타겟 설정
            targetMoveSpeed = moveSpeed;
        }
        else
        {
            // 가까우면 정지 상태
            targetDirection = 0.5f;
            targetMoveSpeed = 0f;
        }

        // SmoothDamp로 방향 부드럽게 변환
        currentDirection = Mathf.SmoothDamp(
            currentDirection,
            targetDirection,
            ref directionVel,
            0.1f
        );

        aniCon.SetFloat(ani_direction, currentDirection);


        // ──────────────────────────────
        // 3) 이동 속도 Lerp (가속/감속)
        // ──────────────────────────────
        currentMoveSpeed = Mathf.Lerp(
            currentMoveSpeed,
            targetMoveSpeed,
            moveSmooth * Time.fixedDeltaTime
        );


        // ──────────────────────────────
        // 4) Translate 이동 (페이드된 속도 적용)
        // ──────────────────────────────
        if (!Mathf.Approximately(currentMoveSpeed, 0f))
        {
            float moveDir = (targetDirection < 0.5f) ? -1f : 1f; // 0f=Left, 1f=Right
            Vector3 moveVector = Vector3.right * moveDir;

            tr.Translate(moveVector * currentMoveSpeed * Time.fixedDeltaTime);
        }
    }


    public void OnStart()
    {
        isMove = true;
    }
}
