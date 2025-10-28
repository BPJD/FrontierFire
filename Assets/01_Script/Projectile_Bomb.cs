using Combat;
using System.Collections;
using UnityEngine;

public class Projectile_Bomb : MonoBehaviour
{
    //SphereCollider bombCol;
    Transform tr;
    Rigidbody rb;
    [SerializeField] ParticleSystem bulletEft;

    [Header("폭발을 한다면")]
    [SerializeField] bool isExplode = false;
    [SerializeField] GameObject hitEft;
    [SerializeField] float explodeRadius = 10f;
    [SerializeField] LayerMask damageableLayers;
    [SerializeField] LayerMask obstacleLayers;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    int bulletDamage = 5;
    float bulletRangeSqr = 10f;

    string unitTagCode = "Unit";

    bool isActivated = false;

    public float explodeTimer = 2f;
    bool isTriggerActivated = false;

    [Header("자체 발사 시스템이라면")]
    [SerializeField] float force = 100f;


    void Awake()
    {
        tr = transform;
        rb = GetComponent<Rigidbody>();
    }

    public void ForceShoot()
    {
        if(rb != null)
        {
            rb.AddForce(tr.forward * force);
        }
    }

    void FixedUpdate()
    {
        if (!isActivated)
        {
            bulletEft.gameObject.SetActive(true);
            bulletEft.Clear(true);
            bulletEft.Play(true);
            isActivated = true;
        }
    }

    

    private void OnCollisionEnter(Collision other)
    {
        if (!isTriggerActivated)
        {
            StartCoroutine(BulletExplode(isExplode));
            isTriggerActivated = true;
        }
    }

    void BulletDisable()
    {
        isActivated = false;
        bulletEft.Clear(true);
        bulletEft.Stop(true);
        gameObject.SetActive(false);
    }

    public void SetBulletStatus(int _damage, float _range)
    {
        bulletDamage = _damage;
        bulletRangeSqr = _range * _range;
    }

    IEnumerator BulletExplode(bool _isExplode)
    {
        yield return new WaitForSeconds(explodeTimer);

        if (_isExplode)
        {

            

            // 폭발 이펙트 생성
            if (hitEft != null)
            {
                GameObject eft = Instantiate(hitEft, tr.position, Quaternion.identity);
                eft.transform.localScale = Vector3.one * explodeRadius;
            }

            // 범위 내에 있는 대상 탐색
            Collider[] hitColliders = Physics.OverlapSphere(tr.position, explodeRadius, damageableLayers);
            float sqrExplosionRadius = explodeRadius * explodeRadius;

            foreach (Collider hit in hitColliders)
            {
                Transform target = hit.transform;

                Vector3 offset = (target.position + Vector3.up) - tr.position;
                float sqrDistance = offset.sqrMagnitude;

                if (sqrDistance <= sqrExplosionRadius)
                {
                    float approxDistance = Mathf.Sqrt(sqrDistance);
                    Vector3 direction = offset.normalized;

                    // Debug용 라인 표시 (Ray와 동일 경로)
                    Debug.DrawLine(tr.position, tr.position + direction * approxDistance, Color.red, 1f); // 1초간 표시

                    if (!Physics.Raycast(tr.position, direction, approxDistance, obstacleLayers))
                    {
                        UnitStatus unit = hit.GetComponent<UnitStatus>();

                        if (unit != null)
                        {
                            Vector3 _colPos = target.position + Vector3.up;

                            var payload = DamagePayload.Create(
                                baseDamage: bulletDamage,
                                ammo: 0,
                                atkType: WeaponParamsSO.AtkTypes.Normal,
                                isCritical: false,
                                isWeakPoint: false,
                                hitPoint: _colPos
                            );

                            unit.TakeDamage(payload);
                        }
                    }
                    else
                    {
                        Debug.Log($"{target.name}은(는) 벽에 가려져 있어 피해 없음");
                    }
                }
            }
        }

        Destroy(gameObject);
    }

    public void MuzzleEftisEnd()
    {
        BulletDisable();
    }

}