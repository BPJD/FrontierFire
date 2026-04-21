using Combat;
using System.Collections;
using UnityEngine;

public class Projectile_ExplodingOrb : MonoBehaviour
{
    Transform tr;
    Rigidbody rb;

    [SerializeField] ParticleSystem bulletEft;

    [Header("폭발 설정")]
    [SerializeField] GameObject hitEft;
    [SerializeField] float explodeRadius = 10f;
    [SerializeField] LayerMask damageableLayers;
    [SerializeField] LayerMask obstacleLayers;

    [Header("이동 설정")]
    [SerializeField] float orbSpeed = 5f;
    [SerializeField] float explodeTimer = 3f;
    [SerializeField] Vector3 offset = new Vector3(0f, 1.5f, 0f);
    

    [Header("보스 고정 피해 설정")]
    [SerializeField] int fixedExplodeDamage = 1000;
    [SerializeField] float defaultUnitDamageMultiplier = 1.5f;

    bool isActivated = false;
    bool isExploded = false;

    Transform target;
    DamagePayload payload;

    private void Awake()
    {
        tr = transform;
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        isActivated = false;
        isExploded = false;
        StartCoroutine(MoveAndExplode());
    }

    void FixedUpdate()
    {
        if (!isActivated)
        {
            if (bulletEft != null)
            {
                bulletEft.gameObject.SetActive(true);
                bulletEft.Clear(true);
                bulletEft.Play(true);
            }

            isActivated = true;
        }

        if (isExploded)
            return;

        if (target != null)
        {
            Vector3 targetPos = target.position + offset;
            targetPos.z = tr.position.z;

            Vector3 dir = (targetPos - tr.position).normalized;
            Vector3 nextPos = tr.position + (dir * orbSpeed * Time.fixedDeltaTime);

            if (rb != null)
            {
                rb.MovePosition(nextPos);
            }
            else
            {
                tr.position = nextPos;
            }
        }
    }

    public void SetBulletStatus(Transform targetTr, DamagePayload newPayload)
    {
        payload = newPayload;

        target = targetTr;
    }

    IEnumerator MoveAndExplode()
    {
        float timer = 0f;

        while (timer < explodeTimer)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Explode();
    }

    void Explode()
    {
        if (isExploded)
            return;

        isExploded = true;

        Vector3 explosionPos = tr.position;

        // 폭발 이펙트
        if (hitEft != null)
        {
            GameObject eft = Instantiate(hitEft, explosionPos, Quaternion.identity);
            eft.transform.localScale = Vector3.one * explodeRadius;
        }

        Collider[] hitColliders = Physics.OverlapSphere(explosionPos, explodeRadius, damageableLayers);
        float sqrExplosionRadius = explodeRadius * explodeRadius;

        foreach (Collider hit in hitColliders)
        {
            if (hit == null)
                continue;

            Vector3 targetPoint = hit.ClosestPoint(explosionPos);
            Vector3 offset = targetPoint - explosionPos;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance > sqrExplosionRadius)
                continue;

            float distance = Mathf.Sqrt(sqrDistance);
            Vector3 direction = distance > 0.0001f ? offset / distance : Vector3.up;

            // 장애물 체크
            if (distance > 0.001f && Physics.Raycast(explosionPos, direction, distance, obstacleLayers))
                continue;

            // 타겟 판정
            UnitWeakPoint unitWeakHit = hit.GetComponent<UnitWeakPoint>();
            UnitStatus unit = hit.GetComponent<UnitStatus>();

            if (unit == null)
            {
                unit = hit.GetComponentInParent<UnitStatus>();
            }

            // 거리 기반 피해 배율
            float normalizedDistance = Mathf.Clamp01(distance / explodeRadius);
            float distanceMultiplier = Mathf.Lerp(1.25f, 0f, normalizedDistance);

            int finalExplodeDamage = 0;

            // Default 타입이면 고정 피해 1000
            if (unit != null && unit.unitParams != null &&
                unit.unitParams.u_type == UnitParamsSO.UnitTypes.Default)
            {
                finalExplodeDamage = fixedExplodeDamage;
            }
            else
            {
                // 그 외에는 payload 피해량 * 거리 보정
                finalExplodeDamage = Mathf.RoundToInt(payload.baseDamage * distanceMultiplier);
            }

            if (finalExplodeDamage <= 0)
                continue;

            var newPayload = payload;
            newPayload.baseDamage = finalExplodeDamage;
            newPayload.hitPoint = targetPoint;

            if (unitWeakHit != null)
            {
                newPayload.isWeakPoint = true;
                unitWeakHit.WeatPointDamage(newPayload);
            }
            else if (unit != null)
            {
                newPayload.isWeakPoint = false;
                unit.TakeDamage(newPayload);
            }
        }

        BulletDisable();
    }

    void BulletDisable()
    {
        isActivated = false;

        if (bulletEft != null)
        {
            bulletEft.Clear(true);
            bulletEft.Stop(true);
        }

        gameObject.SetActive(false);
    }

    public void MuzzleEftisEnd()
    {
        BulletDisable();
    }
}