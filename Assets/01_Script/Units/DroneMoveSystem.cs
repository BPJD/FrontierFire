using UnityEngine;

public class DroneMoveSystem : MonoBehaviour
{
    TurretAttackSystem turretSystem;
    public Transform target { get; private set; }
    public Vector3 followOffset = new Vector3(0.8f, 1.6f, -0.8f);

    [Header("도착 박스(히스테리시스)")]
    public Vector3 enterBoxHalfSize = new Vector3(0.5f, 0.3f, 0.5f); // 이 안으로 들어오면 '도착'
    public Vector3 exitBoxHalfSize = new Vector3(0.7f, 0.5f, 0.7f); // 이 밖으로 나가면 '미도착' 상태로 전환

    [Header("추종 세팅")]
    public float maxSpeed = 10f;
    public float accel = 40f;
    [Range(0f, 1f)] public float damping = 0.06f;

    [Header("장애물 회피")]
    public bool obstacleAvoid = true;
    public LayerMask obstacleMask;
    public float avoidPush = 0.75f;

    [Header("선행 추종")]
    public bool leadTarget = true;
    public float leadTime = 0.12f;

    [Header("박스 내부 코스팅 옵션")]
    public bool coastWithinBox = true;      // 박스 내부에서도 계속 미끄러지도록
    public float minCoastSpeed = 1.5f;      // 박스 내부 최소 속도(>0이면 완전 정지 방지)
    [Range(0f, 1f)] public float coastBlend = 0.5f; // 0=현재 속도 유지, 1=타깃 진행 방향/속도에 더 동화

    Rigidbody rb;
    Rigidbody targetRb;

    // 히스테리시스 상태
    bool isInsideBox = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        turretSystem = GetComponent<TurretAttackSystem>();
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
        targetRb = target.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if(target != null && turretSystem.isPlayerInRange && !turretSystem.isDead)
        {
            DroneMove();
        }
    }

    void DroneMove()
    {
        // 1) 목표 위치
        Vector3 desired = target.position + followOffset;
        if (leadTarget && targetRb != null)
            desired += targetRb.linearVelocity * leadTime;

        // 2) 장애물 회피
        if (obstacleAvoid)
        {
            Vector3 from = rb.position;
            Vector3 dir = desired - from;
            float dist = dir.magnitude;
            if (dist > 0.001f && Physics.Raycast(from, dir.normalized, out var hit, dist, obstacleMask))
                desired += hit.normal * avoidPush;
        }

        // 3) 히스테리시스 기반 박스 내/외부 판정
        Vector3 diff = desired - rb.position;
        diff.y *= 0.3f;
        Vector3 abs = new Vector3(Mathf.Abs(diff.x), Mathf.Abs(diff.y), Mathf.Abs(diff.z));

        if (isInsideBox)
        {
            // 박스 '밖으로' 나갔는지 검사(더 큰 박스 기준)
            isInsideBox = !(abs.x > exitBoxHalfSize.x || abs.y > exitBoxHalfSize.y || abs.z > exitBoxHalfSize.z);
        }
        else
        {
            // 박스 '안으로' 들어왔는지 검사(더 작은 박스 기준)
            isInsideBox = (abs.x < enterBoxHalfSize.x && abs.y < enterBoxHalfSize.y && abs.z < enterBoxHalfSize.z);
        }

        Vector3 desiredVel;

        if (!isInsideBox)
        {
            // 박스 바깥: 목적지로 접근(Arrive)
            float distanceFactor = Mathf.Clamp01(diff.magnitude / (diff.magnitude + 1f));
            float speed = Mathf.Lerp(0f, maxSpeed, distanceFactor);
            desiredVel = diff.normalized * speed;
        }
        else
        {
            // 박스 내부: 코스팅
            if (coastWithinBox)
            {
                // 타깃의 진행 방향/속도와 현재 속도를 블렌딩
                Vector3 targetVel = (targetRb != null ? targetRb.linearVelocity : Vector3.zero);

                // followOffset 방향을 조금 유지하고 싶다면 다음 라인 참고:
                // targetVel += (desired - target.position).normalized * 0.5f;

                Vector3 blended = Vector3.Lerp(rb.linearVelocity, targetVel, coastBlend);

                // 최소 속도 보장(멈추지 않고 살짝 흘러가게)
                float speed = blended.magnitude;
                if (speed < minCoastSpeed)
                {
                    // 현재 이동 방향이 거의 없으면 타깃 쪽으로 살짝
                    Vector3 dir = (speed > 0.001f) ? (blended / speed) : (diff.sqrMagnitude > 0.001f ? diff.normalized : transform.forward);
                    blended = dir * minCoastSpeed;
                }

                // 박스 내부 과속 방지
                desiredVel = Vector3.ClampMagnitude(blended, maxSpeed);
            }
            else
            {
                // 기존처럼 멈추는 쪽으로
                desiredVel = Vector3.zero;
            }
        }

        // 4) 가속 & 감쇠
        Vector3 steering = (desiredVel - rb.linearVelocity) * accel;
        rb.AddForce(steering * Time.fixedDeltaTime, ForceMode.Force);
        rb.linearVelocity *= (1f - damping);
    }

    void OnDrawGizmosSelected()
    {
        if (!target) return;

        Vector3 desired = target.position + followOffset;

        // Enter 박스
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(desired, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, enterBoxHalfSize * 2f);

        // Exit 박스
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 1f);
        Gizmos.DrawWireCube(Vector3.zero, exitBoxHalfSize * 2f);

        // 현재 위치와 연결선
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, desired);
    }
}
