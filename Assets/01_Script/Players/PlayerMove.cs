using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    Transform playerTr;
    //public Transform meshTr;

    Rigidbody playerRig;
    public bool isJumping { get; set; }
    public bool isGrounded { get; private set; } = true;
    public bool externalMoveLock { get; private set; } = false;

    int jumpCount = 0;

    float speedCur = 0f;

    public float frictionFactor = 0.9f; // 감속 계수 (1에 가까울수록 느림)
    float moveSpeed_status = 1f;
    public float moveSpeed_anim { get; private set; } = 0.5f;
    public float jumpPower = 5f;
    public int jumpCountMax = 2;
    public float moveDir { get; private set; } = 0f;
    public float moveDir_y { get; private set; } = 0f;
    float movingCoolCur = 0f;

    bool isMovable = true;
    public bool isSprinting { get; private set; } = false;
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

    AudioSource playerAudio;
    [SerializeField] AudioClip jumpSound_ground;
    [SerializeField] AudioClip jumpSound_air;
    [SerializeField] AudioClip jumpSound_land;


    float velX = 0f; // 실제 적용할 수평 속도( m/s )
    float airVelX = 0f;   // 공중 수평 관성 속도 (m/s)

    void Start()
    {
        playerTr = GetComponent<Transform>();
        playerRig = GetComponent<Rigidbody>();
        aniCon = GetComponentInChildren<Animator>();
        playerStat = GetComponent<UnitStatus>();
        playerAudio = GetComponent<AudioSource>();
        SpeedSet(playerStat.moveSpeed);
    }
    public void SetExternalMoveLock(bool v)
    {
        externalMoveLock = v;

        // 입력에 의한 감속/가속도 같이 멈추고 싶으면 여기서 moveDir=0 같은 것도 가능
    }

    public void SpeedSet(float speed)
    {
        // jumpCountMax만 UnitParams 기반으로 세팅
        jumpCountMax = playerStat.unitParams.u_multijumpCount + 1;

        float _animSpeed;

        if (speed <= 5f)
        {
            float t = Mathf.InverseLerp(2.5f, 5f, speed);
            _animSpeed = Mathf.Lerp(0.25f, 0.5f, t);
        }
        else
        {
            float t = Mathf.InverseLerp(5f, 8f, speed);
            _animSpeed = Mathf.Lerp(0.5f, 1f, t);
        }

        // Clamp (혹시라도 외부 값 들어올 대비)
        moveSpeed_anim = Mathf.Clamp(_animSpeed, 0.25f, 1f);

    }

    private void FixedUpdate()
    {
        if (externalMoveLock) return;

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
        moveDir_y = input.y;
    }

    public void JumpRequested()
    {
        if (externalMoveLock) return;

        // 지상 점프
        if (isGrounded)
        {
            jumpCount = 1;
            playerJump();

            playerAudio.PlayOneShot(jumpSound_ground);
            return;
        }

        // 공중 추가 점프
        if (jumpCount < jumpCountMax)
        {
            jumpCount++;
            playerJump();

            playerAudio.PlayOneShot(jumpSound_air);
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



    /*
    void Update()
    {
        if (externalMoveLock)
        {
            // 애니메이션만 돌진용으로 따로 처리할 거면 여기서 유지
            return;
        }


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
    */

    void Update()
    {
        if (externalMoveLock)
        {
            return;
        }

        // 0) 입력 데드존
        const float inputDeadzone = 0.05f;

        // 1) 이동 배율 계산 (Sprint 제거 → 항상 기본값)
        float baseStatus = 1f;

        float airPenalty = isJumping
            ? airPenaltyNormal
            : 1f;

        moveSpeed_status = baseStatus * airPenalty;

        // 2) 가속 / 감속
        speedCur = Mathf.MoveTowards(speedCur, (moveDir * moveSpeed_status), 3f * Time.deltaTime);

        // 3) 공중 이동
        if (isJumping)
        {
            float targetAir = moveDir * playerStat.moveSpeed * moveSpeed_status;
            airVelX = Mathf.MoveTowards(airVelX, targetAir, airAccel * Time.deltaTime);
        }

        // 4) 애니메이션
        float animVel = isJumping
            ? airVelX / Mathf.Max(playerStat.moveSpeed, 0.0001f)
            : speedCur;

        float _moveAniDir = isLookingRight ? animVel : -animVel;

        // 현재 실제 이동 속도 추출
        float currentSpeed = isJumping
            ? Mathf.Abs(airVelX)
            : Mathf.Abs(speedCur);

        // 방향 반영
        float finalSpeed = Mathf.Clamp(_moveAniDir * moveSpeed_anim, -1f, 1f);

        // 적용
        aniCon.SetFloat("Speed", finalSpeed);

        // 5) 기타
        if (movingCoolCur > 0f)
            movingCoolCur -= Time.deltaTime;
    }


    void playerJump()
    {
        // 공중 진입 직전 스프린트 상태 기록
        bool sprintActiveNow = isSprinting && isSprintable && !isJumping && Mathf.Abs(moveDir) > 0.05f;
        wasSprintingBeforeAir = sprintActiveNow;

        isGrounded = false;
        isJumping = true;
        aniCon.SetBool("IsAir", true);

        // 점프 직전 속도를 공중 속도로 넘김
        airVelX = speedCur * playerStat.moveSpeed;

        float _jumpPower = Mathf.Max(playerStat.jumpPower, 8f);

        playerRig.linearVelocity = new Vector3(
            playerRig.linearVelocity.x,
            _jumpPower,
            playerRig.linearVelocity.z
        );

        if (playerCol && airMaterial)
            playerCol.material = airMaterial;
    }

    public void GroundCheck()
    {
        // 이미 지상 상태면 중복 처리 방지
        if (isGrounded) return;

        // 상승 중에는 착지 처리 금지
        if (playerRig.linearVelocity.y > 0.1f) return;

        isGrounded = true;
        isJumping = false;
        jumpCount = 0;

        aniCon.SetBool("IsAir", false);

        float denom = Mathf.Max(playerStat.moveSpeed, 0.0001f);
        speedCur = Mathf.Clamp(airVelX / denom, -1f, 1f);
        airVelX = 0f;

        if (playerCol && groundMaterial)
            playerCol.material = groundMaterial;

        wasSprintingBeforeAir = false;

        playerAudio.PlayOneShot(jumpSound_land);
    }


    public void PlayerFalling()
    {
        // 이미 공중 상태면 중복 처리 안 함
        if (!isGrounded) return;

        bool sprintActiveNow = isSprinting && isSprintable && !isJumping && Mathf.Abs(moveDir) > 0.05f;
        wasSprintingBeforeAir = sprintActiveNow;

        jumpCount++;

        isGrounded = false;
        isJumping = true;
        aniCon.SetBool("IsAir", true);

        // 낙하는 점프 사용 횟수를 증가시키지 않음
        airVelX = speedCur * playerStat.moveSpeed;

        if (playerCol && airMaterial)
            playerCol.material = airMaterial;

        //Debug.Log("Falling");
    }



}