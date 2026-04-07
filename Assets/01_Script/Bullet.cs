using Combat;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Collider bulletCol;
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


    [Header("착탄 사운드")]
    //[SerializeField] AudioClip hitSound_unit;
    [SerializeField] AudioClip[] hitSound_terrains;
    //[SerializeField] AudioClip hitSound_shield;

    public bool isMove = true;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    bool isCritical = false;
    int bulletDamage = 5;
    float bulletRangeSqr = 500f;
    WeaponParamsSO.AtkTypes atkType = WeaponParamsSO.AtkTypes.Normal;
    float absorptionRate = 0f;

    bool isActivated = false;

    bool isCollided = false;

    UnitStatus shooterUnitStat;



    Vector3 startPos;

    void Awake()
    {
        bulletCol = GetComponent<BoxCollider>();

        if(bulletCol == null)
        {
            bulletCol = GetComponent<SphereCollider>();
        }

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
                hitPoint: _colPos,
                absorption: absorptionRate,
                attackerStat: shooterUnitStat
            );

            BulletExplode(isExplode, payload, _colPos);
            BulletDisable();
        }

    }


    private void OnTriggerEnter(Collider other)
    {
        if(muzzleEft != null)
        {
            muzzleEft.Play(true);
        }

        Vector3 _colPos = tr.position - (tr.forward * 0.3f);

        var payload = DamagePayload.Create(
            baseDamage: bulletDamage,
            ammo: 0,
            atkType: atkType,
            isCritical: isCritical,
            isWeakPoint: false,
            hitPoint: _colPos,
            absorption: absorptionRate,
            attackerStat: shooterUnitStat
        );

        if (other.CompareTag(Data_Strings.UnitTag) || other.CompareTag(Data_Strings.playerTag))
        {
            if (!isExplode)
            {
                other.GetComponent<UnitStatus>().TakeDamage(payload);
            }
        }
        else if (other.CompareTag(Data_Strings.terrainTag))
        {
            if (!isExplode && hitSound_terrains.Length != 0)
            {
                int randIndex = Random.Range(0, hitSound_terrains.Length);
                muzzleEft.gameObject.GetComponent<Bullet_MuzzlePlayer>().PlayMuzzleSound(hitSound_terrains[randIndex]);
            }
        }
        else if (other.CompareTag(Data_Strings.WeakPointTag))
        {
            if (!isExplode)
            {
                other.GetComponent<UnitWeakPoint>().WeatPointDamage(payload);
            }
        }
        else if (other.CompareTag(Data_Strings.shieldTag))
        {
            if (!isExplode)
            {
                other.GetComponent<Shield>().TakeDamage(payload);
            }
        }

        if (bulletMesh != null)
        {
            bulletMesh.SetActive(false);
        }

        BulletExplode(isExplode, payload, _colPos);

        bulletCol.enabled = false;
        isCollided = true;
        bulletEft.gameObject.SetActive(false);
    }

    void BulletDisable()
    {

        if (transform.parent != null)
        {
            isActivated = false;
            bulletEft.Clear(true);
            bulletEft.Stop(true);
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetBulletStatus(int _damage = 0, float _range = 1000f, float _speed = 20f, WeaponParamsSO.AtkTypes _type = WeaponParamsSO.AtkTypes.Normal, bool isCri = false, float explodeRad = 0f, float absorption = 0f, UnitStatus shooterStat = null)
    {
        isCritical = isCri;
        bulletDamage = _damage;
        bulletRangeSqr = _range * _range;
        atkType = _type;
        absorptionRate = absorption;
        shooterUnitStat = shooterStat;

        if (explodeRad != 0f)
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

    void BulletExplode(bool _isExplode, DamagePayload payload, Vector3 explosionPos)
    {
        if (!_isExplode)
            return;

        // 폭발 이펙트 생성
        if (hitEft != null)
        {
            GameObject eft = Instantiate(hitEft, explosionPos, Quaternion.identity);
            eft.transform.localScale = Vector3.one * explodeRadius;
        }

        // 범위 내에 있는 대상 탐색
        Collider[] hitColliders = Physics.OverlapSphere(explosionPos, explodeRadius, damageableLayers);
        float sqrExplosionRadius = explodeRadius * explodeRadius;

        foreach (Collider hit in hitColliders)
        {
            if (hit == null)
                continue;

            // 콜라이더 실제 표면 기준점 사용
            Vector3 targetPoint = hit.ClosestPoint(explosionPos);
            Vector3 offset = targetPoint - explosionPos;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance > sqrExplosionRadius)
                continue;

            float distance = Mathf.Sqrt(sqrDistance);
            if (distance <= 0.001f)
                distance = 0.001f;

            Vector3 direction = offset.normalized;

            // 장애물에 가려졌는지 확인
            if (Physics.Raycast(explosionPos, direction, distance, obstacleLayers))
                continue;

            // WeakPoint 우선 판정
            UnitWeakPoint unitWeakHit = hit.GetComponent<UnitWeakPoint>();
            UnitStatus unit = hit.GetComponent<UnitStatus>();

            var newPayload = payload;
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
    }

    public void MuzzleEftisEnd()
    {
        BulletDisable();
    }

}