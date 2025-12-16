using Combat;
using System.Collections;
using UnityEngine;

public class BossGiantAttackSystem : MonoBehaviour
{
    BossGiantAttackControl giantAI;
    UnitStatus unitStat;
    Transform target;

    [SerializeField] float swingDamageRevision = 1f;

    [SerializeField] float smashDamageRevision = 1f;
    [SerializeField] float stoneDamageRevision = 1f;

    [SerializeField] float stompDamageRevision = 1f;
    [SerializeField] float dripStoneDamageRevision = 1f;

    [SerializeField] int stone_attackCount = 12;
    [SerializeField] int dripStone_attackCount = 4;
    [SerializeField] int mobSpawn_Count = 3;

    [Header("Stone Settings")]
    [SerializeField] float stoneFireDelay = 0.2f;
    [SerializeField] float stoneRandRange = 3.5f;

    [Header("DripStone Settings")]
    [SerializeField] float dripStoneFireDelay = 0.5f;
    [SerializeField] float dripStoneRandRange = 2.5f;

    [SerializeField] GameObject shockEft;
    [SerializeField] float shockRadius = 2f;
    [SerializeField] LayerMask damageableLayers;
    [SerializeField] LayerMask obstacleLayers;
    Transform tr;

    StageModule stageCon;
    Transform stageConTr;

    BossGiant_TargetBox targetBox;

    [SerializeField] Transform leftFoot, rightHand;


    void Start()
    {
        stageCon = GetComponentInParent<StageModule>();
        stageConTr = stageCon.gameObject.transform;
        giantAI = GetComponentInParent<BossGiantAttackControl>();
        unitStat = GetComponentInParent<UnitStatus>();
        target = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).transform;
        tr = transform;
        targetBox = GetComponentInChildren<BossGiant_TargetBox>();
    }


    DamagePayload DamagePayLoad(float damageRevision)
    {
        var payload = DamagePayload.Create(
        baseDamage: (int)(unitStat.atkCur * damageRevision),
        ammo: 0,
        atkType: WeaponParamsSO.AtkTypes.Normal,
        isCritical: false,
        isWeakPoint: false,
        hitPoint: target.position + Vector3.up
        );

        return payload;
    }



    public void SmashPattern()
    {
        StartCoroutine(StoneFire());
        ShockExplode(DamagePayLoad(smashDamageRevision), rightHand.position);
    }

    public void StompPattern()
    {
        StartCoroutine(EnemyGenerate());
        ShockExplode(DamagePayLoad(stompDamageRevision), leftFoot.position);
    }

    public void DripStonePattern()
    {
        StartCoroutine(DripStoneFire());
    }

    public void SwingPattern()
    {
        if (targetBox.isPlayerInBox)
        {
            target.gameObject.GetComponent<UnitStatus>().TakeDamage(DamagePayLoad(swingDamageRevision));
        }
    }

    GameObject GetProjectile(bool isDripStone)
    {
        GameObject _projectile;
        if (isDripStone)
        {
            _projectile = giantAI.dripStoneProjectiles[Random.Range(0, giantAI.dripStoneProjectiles.Length)];
        }
        else
        {
            _projectile = giantAI.stoneProjectiles[Random.Range(0, giantAI.stoneProjectiles.Length)];
        }
        return _projectile;
    }

    IEnumerator StoneFire()
    {
        for (int i = 0; i < stone_attackCount; i++)
        {
            float randX = Random.Range(-stoneRandRange, stoneRandRange);
            Vector3 _randPos = new Vector3(target.position.x, 14.5f, 0f);
            _randPos.x += randX;

            GameObject _bullet = Instantiate(GetProjectile(false), _randPos, Quaternion.identity);
            _bullet.GetComponent<Bullet>().SetBulletStatus(
                (int)(unitStat.atkCur * stoneDamageRevision),
                50f,
                0f,
                WeaponParamsSO.AtkTypes.Normal,
                false,
                0f,
                0f,
                unitStat);

            yield return new WaitForSeconds(stoneFireDelay);
        }

    }

    IEnumerator DripStoneFire()
    {
        for (int i = 0; i < dripStone_attackCount; i++)
        {
            float randX = Random.Range(-dripStoneRandRange, dripStoneRandRange);
            Vector3 _randPos = new Vector3(target.position.x, 14.5f, 0f);
            _randPos.x += randX;

            GameObject _bullet = Instantiate(GetProjectile(true), _randPos, Quaternion.identity);
            _bullet.GetComponent<Bullet>().SetBulletStatus(
                (int)(unitStat.atkCur * dripStoneDamageRevision),
                50f,
                0f,
                WeaponParamsSO.AtkTypes.Normal,
                false,
                0f,
                0f,
                unitStat
                );

            yield return new WaitForSeconds(dripStoneFireDelay);
        }

    }

    IEnumerator EnemyGenerate()
    {
        for (int i = 0; i < mobSpawn_Count; i++)
        {
            Transform[] _positions = giantAI.mobSpawnPoints;
            Shuffle<Transform>(_positions);

            Vector3 _spawnPos = _positions[i].position;
            GameObject _unit = giantAI.spawnMobs[Random.Range(0, giantAI.spawnMobs.Length)];

            Instantiate(_unit, _spawnPos, Quaternion.identity, stageConTr);
            yield return new WaitForSeconds(dripStoneFireDelay);
        }
    }

    void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1); // 0 ~ i

            // swap
            T temp = array[i];
            array[i] = array[rand];
            array[rand] = temp;
        }
    }

    void ShockExplode(DamagePayload payload, Vector3 eftPos)
    {
        // 폭발 이펙트 생성
        if (shockEft != null)
        {
            GameObject eft = Instantiate(shockEft, eftPos, Quaternion.identity);
            eft.transform.localScale = Vector3.one * shockRadius;
        }

        // 범위 내에 있는 대상 탐색
        Collider[] hitColliders = Physics.OverlapSphere(eftPos, shockRadius, damageableLayers);
        float sqrExplosionRadius = shockRadius * shockRadius;

        foreach (Collider hit in hitColliders)
        {
            Transform target = hit.transform;

            Vector3 offset = (target.position + Vector3.up) - eftPos;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance <= sqrExplosionRadius)
            {
                float approxDistance = Mathf.Sqrt(sqrDistance);
                Vector3 direction = offset.normalized;

                // Debug용 라인 표시 (Ray와 동일 경로)
                Debug.DrawLine(eftPos, eftPos + direction * approxDistance, Color.red, 1f); // 1초간 표시

                if (!Physics.Raycast(eftPos, direction, approxDistance, obstacleLayers))
                {
                    UnitStatus unit = hit.GetComponent<UnitStatus>();
                    UnitWeakPoint unitWeakHit = hit.GetComponent<UnitWeakPoint>();

                    if (unit != null)
                    {
                        unit.TakeDamage(payload);
                    }
                    else if (unitWeakHit != null)
                    {
                        unitWeakHit.WeatPointDamage(payload);
                    }
                }
                else
                {

                }
            }
        }
    }
}
