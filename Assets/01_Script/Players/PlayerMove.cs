using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    Transform playerTr;
    //public Transform meshTr;

    Rigidbody playerRig;
    public bool isJumping { get; set; }

    int jumpCount = 0;

    float speedCur = 0f;

    public float frictionFactor = 0.9f; // 감속 계수 (1에 가까울수록 느림)
    float moveSpeed_status = 1f;
    public float jumpPower = 5f;
    public int jumpCountMax = 2;
    float moveDir = 0f;
    float movingCoolCur = 0f;

    bool isMovable = true;
    bool isSprinting = false;
    public bool isAiming = false;
    public bool isSprintable { private get; set; } = true;
    public bool isLookingRight = false;

    public bool isLeftBlocked = false;
    public bool isRightBlocked = false;

    Animator aniCon;
    UnitStatus playerStat;

    [SerializeField] CapsuleCollider playerCol;
    [SerializeField] PhysicsMaterial groundMaterial;
    [SerializeField] PhysicsMaterial airMaterial;

    // ---- 추가: 가감속 파라미터 ----
    [Header("Move Tuning")]
    [SerializeField] float groundAccel = 18f;  // 지상 가속
    [SerializeField] float groundDecel = 24f;  // 지상 감속(브레이크)
    [SerializeField] float airAccel = 5f;   // 공중 조작력


    // ---- 공중 패널티 고정 완화 ----
    [SerializeField] float airPenaltyNormal = 0.5f;   // 기본 공중 패널티
    [SerializeField] float airPenaltyCarried = 0.75f; // 스프린트 관성 시 공중 패널티(착지 전까지 유지)
    bool wasSprintingBeforeAir = false;               // 공중 진입 직전 스프린트 여부


    float velX = 0f; // 실제 적용할 수평 속도( m/s )
    float airVelX = 0f;   // 공중 수평 관성 속도 (m/s)

    void Start()
    {
        playerTr = GetComponent<Transform>();
        playerRig = GetComponent<Rigidbody>();
        aniCon = GetComponentInChildren<Animator>();
        playerStat = GetComponent<UnitStatus>();
        SpeedSet();
    }

    public void SpeedSet()
    {
        // jumpCountMax만 UnitParams 기반으로 세팅
        jumpCountMax = playerStat.unitParams.u_multijumpCount;
    }

    private void FixedUpdate()
    {
        // 지상/공중에 따라 사용할 수평 속도 선택
        float horizontalSpeed = isJumping
            ? airVelX                        // 공중: 점프 직전 속도를 그대로 사용(관성)
            : speedCur * playerStat.moveSpeed; // 지상: 기존 계산

        float step = horizontalSpeed * Time.fixedDeltaTime;

        if (!isLeftBlocked && !isRightBlocked ||
            (isLeftBlocked && step > 0f) ||
            (isRightBlocked && step < 0f))
        {
            playerRig.MovePosition(playerTr.position + playerTr.right * step);
        }

    }

    public void MoveRequested(Vector2 input)
    {
        moveDir = input.x;
    }

    public void JumpRequested()
    {
        if (!isJumping || jumpCount < jumpCountMax)
        {
            playerJump();
            jumpCount++;
        }
    }


    public void SprintStartRequested()
    {
        if (!isJumping && isSprintable) isSprinting = true;
    }

    public void SprintEndRequested()
    {
        isSprinting = false;
    }

    public void SprintingRequested()
    { 
        if (!isJumping && isSprintable) isSprinting = true;
    } // hold일 경우




    void Update()
    {
        // 0) 입력 데드존(너무 미세한 입력은 무시)
        const float inputDeadzone = 0.05f;

        // 1) 실제로 스프린트가 "유효한지" 계산
        bool sprintActive = isSprinting && isSprintable && !isAiming && !isJumping && Mathf.Abs(moveDir) > inputDeadzone;

        // 2) 이동 배율은 한 곳에서만 계산 (고정 완화)
        //    - 지상 스프린트 1.5배
        //    - 공중: 공중 진입 직전 스프린트였다면 완화(airPenaltyCarried), 아니면 기본(airPenaltyNormal)
        float baseStatus = sprintActive ? 1.5f : 1f;
        float airPenalty = isJumping
            ? (wasSprintingBeforeAir ? airPenaltyCarried : airPenaltyNormal)
            : 1f;
        moveSpeed_status = baseStatus * airPenalty;

        // 3) 가속/감속
        speedCur = Mathf.MoveTowards(speedCur, (moveDir * moveSpeed_status), 3f * Time.deltaTime);

        if (isJumping)
        {
            float targetAir = moveDir * playerStat.moveSpeed * moveSpeed_status;
            airVelX = Mathf.MoveTowards(airVelX, targetAir, airAccel * Time.deltaTime);
        }

        // 애니메이션
        float animVel = isJumping ? airVelX / Mathf.Max(playerStat.moveSpeed, 0.0001f) : speedCur;
        float _moveAniDir = isLookingRight ? animVel : -animVel;
        aniCon.SetFloat("Speed", sprintActive ? _moveAniDir : _moveAniDir * 0.5f);

        if (movingCoolCur > 0f) movingCoolCur -= Time.deltaTime;
    }


    void playerJump()
    {
        // 공중 진입 직전 스프린트 상태 기록 (입력 데드존 동일 적용)
        bool sprintActiveNow = isSprinting && isSprintable && !isJumping && Mathf.Abs(moveDir) > 0.05f;
        wasSprintingBeforeAir = sprintActiveNow;

        isJumping = true;
        aniCon.SetBool("IsAir", true);

        // 점프 직전 지상 수평속도를 공중 속도로 고정 (관성 보존)
        airVelX = speedCur * playerStat.moveSpeed;

        playerRig.linearVelocity = new Vector3(
            playerRig.linearVelocity.x,
            playerStat.jumpPower,
            playerRig.linearVelocity.z
        );

        if (playerCol && airMaterial)
            playerCol.material = airMaterial;
    }

    public void GroundCheck()
    {
        // 상태 전환은 호출부가 보증한다고 보고 그대로 유지
        isJumping = false;
        jumpCount = 0;

        float vy = playerRig.linearVelocity.y;
        //if (vy <= 0f) // 필요시 -0.1f 등으로 조절
            aniCon.SetBool("IsAir", false);

        // 기존 속도 동기화 로직 유지
        float denom = Mathf.Max(playerStat.moveSpeed, 0.0001f);
        speedCur = Mathf.Clamp(airVelX / denom, -1f, 1f);
        airVelX = 0f;

        if (playerCol && groundMaterial) playerCol.material = groundMaterial;

        wasSprintingBeforeAir = false;
    }


    public void PlayerFalling()
    {
        // 지상 → 낙하 전환 시에도 스프린트 관성 인정
        bool sprintActiveNow = isSprinting && isSprintable && !isJumping && Mathf.Abs(moveDir) > 0.05f;
        wasSprintingBeforeAir = sprintActiveNow;

        if (!isJumping) { jumpCount++; }
        isJumping = true;
        aniCon.SetBool("IsAir", true);

        // 지상 속도를 공중 관성으로 넘겨 끊김 방지
        airVelX = speedCur * playerStat.moveSpeed;

        if (playerCol && airMaterial) playerCol.material = airMaterial;
        Debug.Log("Falling");
    }


}