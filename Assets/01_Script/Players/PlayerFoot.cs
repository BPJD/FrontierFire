using UnityEngine;

public class PlayerFoot : MonoBehaviour
{
    [SerializeField] PlayerMove playerMove;
    [SerializeField] LayerMask groundMask;        // ← Ground 전용 레이어
    [SerializeField] float coyoteTime = 0.08f;    // ← 버퍼
    int groundContacts = 0;
    float leaveAt = -1f;

    void Reset()
    {
        if (!playerMove) playerMove = GetComponentInParent<PlayerMove>();
    }

    void Start()
    {
        if (!playerMove) playerMove = GetComponentInParent<PlayerMove>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsGround(other)) return;
        groundContacts++;
        // 트리거 진입 순간에도 착지 시도 (상승 중이면 GroundCheck 내부 조건으로 무시됨)
        if (playerMove.isJumping)
        {
            playerMove.GroundCheck();
        }
        
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsGround(other)) return;
        if (playerMove.isJumping)
        {
            playerMove.GroundCheck(); // 하강/정지 구간에서만 착지됨
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsGround(other)) return;

        groundContacts = Mathf.Max(0, groundContacts - 1);
        if (groundContacts == 0)
        {
            // 바로 떨어졌다고 처리하지 말고 약간의 버퍼를 둠
            leaveAt = Time.time + coyoteTime;
        }
    }

    void Update()
    {
        // 코요테 타임이 지나면 실제 낙하 처리
        if (groundContacts == 0 && leaveAt > 0f && Time.time >= leaveAt)
        {
            playerMove.PlayerFalling();
            leaveAt = -1f;
        }
        else if (groundContacts > 0)
        {
            leaveAt = -1f; // 땅을 다시 밟으면 버퍼 리셋
        }
    }

    bool IsGround(Collider col)
    {
        // 레이어 마스크 기준
        return (groundMask.value & (1 << col.gameObject.layer)) != 0;
        // 태그를 계속 쓰고 싶다면:
        // return col.CompareTag("Terrain");
    }
}