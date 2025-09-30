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

    int bulletDamage = 5;
    float bulletRangeSqr = 10f;

    string unitTagCode = "Unit";

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
            BulletExplode(isExplode);
            BulletDisable();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        muzzleEft.Play(true);
        if (other.CompareTag(unitTagCode) || other.CompareTag("Player") && !isExplode)
        {
            other.GetComponent<UnitStatus>().UnitGetDamage(bulletDamage, 0, 0);
        }

        if (bulletMesh != null)
        {
            bulletMesh.SetActive(false);
        }
        BulletExplode(isExplode);

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

    public void SetBulletStatus(int _damage, float _range)
    {
        bulletDamage = _damage;
        bulletRangeSqr = _range * _range;

        if (isRangeUnlimit)
        {
            bulletRangeSqr = 20000f;
        }

    }

    void BulletExplode(bool _isExplode)
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
                        if (unit != null)
                            unit.UnitGetDamage(bulletDamage, 0, 0);
                    }
                    else
                    {
                        Debug.Log($"{target.name}은(는) 벽에 가려져 있어 피해 없음");
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