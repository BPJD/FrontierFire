using Combat;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    BoxCollider bulletCol;
    Transform tr;
    Rigidbody rb;
    [SerializeField] float bulletSpeed = 20f;
    [SerializeField] ParticleSystem bulletEft;
    [SerializeField] ParticleSystem muzzleEft;

    [SerializeField] bool isRangeUnlimit = false;

    [Header("폭발을 한다면")]
    [SerializeField] bool isExplode = false;
    [SerializeField] GameObject hitEft;
    [SerializeField] float explodeRadius = 10f;
    [SerializeField] LayerMask damageableLayers;
    [SerializeField] LayerMask obstacleLayers;

    [SerializeField] GameObject bulletMesh;

    public bool isMove = true;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    bool isCritical = false;
    int bulletDamage = 5;
    float bulletRangeSqr = 10f;
    WeaponParamsSO.AtkTypes atkType = WeaponParamsSO.AtkTypes.Normal;

    bool isActivated = false;

    bool isCollided = false;



    Vector3 startPos;

    void Awake()
    {
        bulletCol = GetComponent<BoxCollider>();
        tr = transform;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!isActivated)
        {
            if (bulletMesh != null)
            {
                bulletMesh.SetActive(true);
            }

            bulletCol.enabled = true;
            bulletEft.gameObject.SetActive(true);
            startPos = tr.position;
            isCollided = false;
            bulletEft.Clear(true);
            bulletEft.Play(true);
            isActivated = true;
        }

        if (!isCollided && isMove)
        {
            rb.MovePosition(tr.position + tr.forward * bulletSpeed * Time.fixedDeltaTime);
        }

        //tr.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);

        float flyDistance = (tr.position - startPos).sqrMagnitude;
        if (flyDistance > bulletRangeSqr)
        {

            Vector3 _colPos = tr.position;

            var payload = DamagePayload.Create(
                baseDamage: bulletDamage,
                ammo: 0,
                atkType: atkType,
                isCritical: isCritical,
                isWeakPoint: false,
                hitPoint: _colPos
            );

            BulletExplode(isExplode, payload);
            BulletDisable();
        }

    }


    private void OnTriggerEnter(Collider other)
    {
        muzzleEft.Play(true);

        Vector3 _colPos = tr.position - (tr.forward * 0.3f);

        var payload = DamagePayload.Create(
            baseDamage: bulletDamage,
            ammo: 0,
            atkType: atkType,
            isCritical: isCritical,
            isWeakPoint: false,
            hitPoint: _colPos
        );



        if (other.CompareTag(Data_Strings.UnitTag) || other.CompareTag(Data_Strings.playerTag))
        {
            if (!isExplode)
            {
                other.GetComponent<UnitStatus>().TakeDamage(payload);
            }
        }
        else if (other.CompareTag(Data_Strings.WeakPointTag))
        {
            if (!isExplode)
            {
                other.GetComponent<UnitWeakPoint>().WeatPointDamage(payload);
            }
        }

        if (bulletMesh != null)
        {
            bulletMesh.SetActive(false);
        }
        BulletExplode(isExplode, payload);

        bulletCol.enabled = false;
        isCollided = true;
        bulletEft.gameObject.SetActive(false);

        
    }

    void BulletDisable()
    {
        isActivated = false;
        bulletEft.Clear(true);
        bulletEft.Stop(true);
        gameObject.SetActive(false);
    }

    public void SetBulletStatus(int _damage, float _range, float _speed, WeaponParamsSO.AtkTypes _type, bool isCri, float explodeRad)
    {
        isCritical = isCri;
        bulletDamage = _damage;
        bulletRangeSqr = _range * _range;
        atkType = _type;

        if(explodeRad != 0f)
        {
            explodeRadius = explodeRad;
        }

        if(_speed >= 1f)
        {
            bulletSpeed = _speed;
        }

        if (isRangeUnlimit)
        {
            bulletRangeSqr = 20000f;
        }

    }

    void BulletExplode(bool _isExplode, DamagePayload payload)
    {
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
                        UnitWeakPoint unitWeakHit = hit.GetComponent<UnitWeakPoint>();

                        if (unit != null)
                        {
                            var newPayload = payload;
                            newPayload.hitPoint = target.position + Vector3.up;

                            unit.TakeDamage(newPayload);
                        }
                        else if (unitWeakHit != null)
                        {
                            var newPayload = payload;
                            newPayload.hitPoint = target.position + Vector3.up;

                            unitWeakHit.WeatPointDamage(newPayload);
                        }
                    }
                    else
                    {
                        
                    }
                }
            }

        }
    }

    public void MuzzleEftisEnd()
    {
        BulletDisable();
    }

}