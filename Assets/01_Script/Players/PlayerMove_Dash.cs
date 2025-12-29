using UnityEngine;
using System.Collections;

public class PlayerMove_Dash : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerMove moveSystem;
    [SerializeField] GameObject atkCollider;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 18f;
    [SerializeField] float dashDuration = 0.18f;
    [SerializeField] float skin = 0.02f;

    [Header("Collision")]
    [SerializeField] LayerMask groundMask;      // 지형만
    [SerializeField] float castRadius = 0.35f;  // 플레이어 반지름에 맞춰
    [SerializeField] float castHeight = 1.6f;   // 캡슐 높이에 맞춰

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
        hitMaskDefault = rb.includeLayers;
        dashEft = GetComponentInChildren<ParticleSystem>();
    }

    // 입력에서 호출
    public void TryDash(Vector3 inputDirWorld)
    {
        if (IsDashing) return;

        if (inputDirWorld.sqrMagnitude < 0.0001f)
            inputDirWorld = transform.forward;

        dashDir = inputDirWorld.normalized;

        BeginDash();
    }

    void BeginDash()
    {
        IsDashing = true;
        dashTimeLeft = dashDuration;
        rb.excludeLayers = hitMaskignore;

        moveSystem.SetExternalMoveLock(true);

        prevUseGravity = rb.useGravity;
        rb.useGravity = false;

        atkCollider.SetActive(true);

        AudioClip _clip = clip_Dashes[Random.Range(0, clip_Dashes.Length)];
        soundPlayer.PlayOneShot(_clip);
        dashEft.Play(true);

        // 시작 틱부터 속도 고정(원하면 제거 가능)
        rb.linearVelocity = dashDir * dashSpeed;
    }

    void FixedUpdate()
    {
        if (!IsDashing) return;

        float step = dashSpeed * Time.fixedDeltaTime;

        // 지형 충돌 검사(지형만)
        if (CheckHitGround(step, out RaycastHit hit))
        {
            Vector3 stopPos = rb.position + dashDir * Mathf.Max(0f, hit.distance - skin);
            rb.MovePosition(stopPos);
            //EndDashFixed();
            //return;
        }

        // 이동
        rb.MovePosition(rb.position + dashDir * step);

        // 시간 감소
        dashTimeLeft -= Time.fixedDeltaTime;
        if (dashTimeLeft <= 0f)
        {
            EndDashFixed();
        }
    }

    void EndDashFixed()
    {
        if (!IsDashing) return;

        // 속도 정리
        rb.linearVelocity = Vector3.zero;

        // 중력 복구
        rb.useGravity = prevUseGravity;

        IsDashing = false;
        rb.excludeLayers = hitMaskDefault;
        atkCollider.SetActive(false);
        dashEft.Stop(true);

        // 핵심: 같은 Fixed 틱에서 바로 unlock 하면 PlayerMove가 바로 1회 움직일 수 있음
        // 한 틱 뒤에 unlock 해서 겹침/튐 방지
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
}
