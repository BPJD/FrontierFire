using UnityEngine;
using System.Collections;

public class PlayerMove_Dash : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerMove moveSystem;
    [SerializeField] GameObject atkCollider;
    [SerializeField] CapsuleCollider capsule;   // 추가

    [Header("Dash")]
    [SerializeField] float dashSpeed = 18f;
    [SerializeField] float dashDuration = 0.18f;
    [SerializeField] float skin = 0.02f;

    [Header("Collision")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float castRadius = 0.35f;
    [SerializeField] float castHeight = 1.6f;

    LayerMask hitMaskDefault;
    [SerializeField] LayerMask hitMaskignore;

    [SerializeField] AudioClip[] clip_Dashes;
    [SerializeField] AudioSource soundPlayer;
    [SerializeField] ParticleSystem dashEft;

    public bool IsDashing { get; private set; }

    bool prevUseGravity;
    Vector3 dashDir;
    float dashTimeLeft;

    void Reset()
    {
        rb = GetComponentInParent<Rigidbody>();
        moveSystem = GetComponentInParent<PlayerMove>();
        capsule = GetComponentInParent<CapsuleCollider>();
        hitMaskDefault = rb.includeLayers;
        dashEft = GetComponentInChildren<ParticleSystem>();
    }

    void Awake()
    {
        if (!rb) rb = GetComponentInParent<Rigidbody>();
        if (!moveSystem) moveSystem = GetComponentInParent<PlayerMove>();
        if (!capsule) capsule = GetComponentInParent<CapsuleCollider>();

        // 보조: 고속 이동 시 뚫림 완화
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void TryDash(Vector3 inputDirWorld)
    {
        if (IsDashing) return;

        inputDirWorld.z = 0f;

        if (inputDirWorld.sqrMagnitude < 0.0001f)
            inputDirWorld = transform.forward; // 또는 마지막 이동 방향

        dashDir = inputDirWorld.normalized;

        BeginDash();
    }

    void BeginDash()
    {
        IsDashing = true;
        dashTimeLeft = dashDuration;

        // 충돌 레이어 무시
        rb.excludeLayers = hitMaskignore;

        moveSystem.SetExternalMoveLock(true);

        prevUseGravity = rb.useGravity;
        rb.useGravity = false;

        atkCollider.SetActive(true);

        AudioClip _clip = clip_Dashes[Random.Range(0, clip_Dashes.Length)];
        soundPlayer.PlayOneShot(_clip);
        dashEft.Play(true);

        // ★ 중요: 시작 시점 겹침 해소 (벽에 붙어서 대시할 때 특히)
        ResolveInitialOverlap();

        // 속도 고정 (원하면 유지)
        rb.linearVelocity = dashDir * dashSpeed;
    }

    void FixedUpdate()
    {
        if (!IsDashing) return;

        float totalStep = dashSpeed * Time.fixedDeltaTime;

        // 한 틱에 1~2번 정도 “충돌-슬라이드”를 처리하면 코너에서 안정적
        float remaining = totalStep;

        // 안전장치: 너무 많은 루프 방지w
        const int maxIters = 2;

        for (int iter = 0; iter < maxIters && remaining > 0.0001f; iter++)
        {
            // 현재 dashDir 기준으로 충돌 검사
            if (CheckHitGround(remaining, out RaycastHit hit))
            {
                float moveDist = Mathf.Max(0f, hit.distance - skin);

                // 1) 일단 부딪히기 직전까지 이동 (MovePosition은 루프당 1번)
                if (moveDist > 0.0001f)
                {
                    rb.MovePosition(rb.position + dashDir * moveDist);
                    remaining -= moveDist;
                }
                else
                {
                    // 거의 붙어있는 상태: 일단 아주 살짝 표면 밖으로 밀어내면
                    // “접촉인데도 0거리 히트”가 계속 나오는 걸 줄여줌
                    rb.MovePosition(rb.position + hit.normal * skin);
                    remaining = Mathf.Max(0f, remaining - skin);
                }

                // 2) 슬라이드 방향 계산: 벽 법선에 수직인 성분만 남김
                Vector3 slide = Vector3.ProjectOnPlane(dashDir, hit.normal);

                // 정면으로 박았거나 모서리 끼임이면 slide가 거의 0이 됨
                if (slide.sqrMagnitude < 0.0001f)
                {
                    // 여기서 “대시 유지”를 원해도, 사실상 갈 곳이 없어서
                    // 계속 밀면 다시 뚫림/떨림이 생길 수 있음 → 종료가 가장 안전
                    EndDashFixed();
                    return;
                }

                dashDir = slide.normalized;

                // 루프 계속 → 남은 거리로 새 dashDir 방향 이동 시도
                continue;
            }
            else
            {
                // 충돌 없음: 남은 거리 그냥 이동하고 끝
                rb.MovePosition(rb.position + dashDir * remaining);
                remaining = 0f;
                break;
            }
        }

        dashTimeLeft -= Time.fixedDeltaTime;
        if (dashTimeLeft <= 0f)
            EndDashFixed();
    }


    void EndDashFixed()
    {
        if (!IsDashing) return;

        rb.linearVelocity = Vector3.zero;
        rb.useGravity = prevUseGravity;

        IsDashing = false;
        rb.excludeLayers = hitMaskDefault;

        atkCollider.SetActive(false);
        dashEft.Stop(true);

        StartCoroutine(UnlockNextFixed());
    }

    IEnumerator UnlockNextFixed()
    {
        yield return new WaitForFixedUpdate();
        moveSystem.SetExternalMoveLock(false);
    }

    bool CheckHitGround(float step, out RaycastHit hit)
    {
        Vector3 up = Vector3.up;
        Vector3 p1 = rb.position + up * (castRadius);
        Vector3 p2 = rb.position + up * (castHeight - castRadius);

        return Physics.CapsuleCast(
            p1, p2, castRadius,
            dashDir,
            out hit,
            step + skin,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    void ResolveInitialOverlap()
    {
        if (!capsule) return;

        // 캡슐의 월드 캡슐 끝점 계산 (Collider 기준)
        GetWorldCapsule(capsule, out Vector3 a, out Vector3 b, out float r);

        // 겹침 대상 탐색
        Collider[] overlaps = Physics.OverlapCapsule(
            a, b, r,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        // 여러 개 겹치면 누적해서 밀어낼 수 있게 약간 반복
        // (대부분 1~2회면 충분)
        for (int iter = 0; iter < 2; iter++)
        {
            bool pushedAny = false;

            foreach (var other in overlaps)
            {
                if (!other) continue;
                if (other == capsule) continue;

                if (Physics.ComputePenetration(
                    capsule, capsule.transform.position, capsule.transform.rotation,
                    other, other.transform.position, other.transform.rotation,
                    out Vector3 dir, out float dist))
                {
                    // skin만큼 더 밀어냄
                    rb.MovePosition(rb.position + dir * (dist + skin));
                    pushedAny = true;
                }
            }

            if (!pushedAny) break;

            // 새 위치 기준으로 재계산
            GetWorldCapsule(capsule, out a, out b, out r);
            overlaps = Physics.OverlapCapsule(a, b, r, groundMask, QueryTriggerInteraction.Ignore);
        }
    }

    static void GetWorldCapsule(CapsuleCollider col, out Vector3 a, out Vector3 b, out float r)
    {
        Transform t = col.transform;

        // radius는 스케일 반영
        float scaleX = Mathf.Abs(t.lossyScale.x);
        float scaleZ = Mathf.Abs(t.lossyScale.z);
        float maxXZ = Mathf.Max(scaleX, scaleZ);
        r = col.radius * maxXZ;

        // height도 스케일 반영(캡슐 축: Y 기준이라고 가정)
        float scaleY = Mathf.Abs(t.lossyScale.y);
        float h = Mathf.Max(col.height * scaleY, r * 2f);

        Vector3 center = t.TransformPoint(col.center);

        // 캡슐의 원통 구간 절반 길이
        float half = (h * 0.5f) - r;
        Vector3 up = t.up;

        a = center + up * half;
        b = center - up * half;
    }
}
