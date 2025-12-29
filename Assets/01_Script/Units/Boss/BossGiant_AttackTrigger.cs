

using Combat;
using NUnit.Framework.Interfaces;
using System.Collections;
using UnityEngine;

public class BossGiant_AttackTrigger : MonoBehaviour
{

    [SerializeField] BossGiantAttackControl.GiantPattern pattern;
    BossGiantAttackControl giantAI;
    BossGiantMove giantMove;
    UnitStatus unitStat;
    Transform target;

    [SerializeField] float damageRevision = 1f;
    GameObject projectile;
    [SerializeField] int attackCount = 3;
    [SerializeField] float projectileFireDelay = 0.5f;
    [SerializeField] float projectileRandRange = 2.5f;

    [SerializeField] GameObject shockEft;
    [SerializeField] float shockRadius = 2f;
    [SerializeField] LayerMask damageableLayers;
    [SerializeField] LayerMask obstacleLayers;
    Transform tr;

    StageModule stageCon;
    Transform stageConTr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stageCon = GetComponentInParent<StageModule>();
        stageConTr = stageCon.gameObject.transform;
        giantAI = GetComponentInParent<BossGiantAttackControl>();
        unitStat = GetComponentInParent<UnitStatus>();
        target = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).transform;
        giantMove = GetComponentInParent<BossGiantMove>();
        tr = transform;


        switch (pattern)
        {
            case BossGiantAttackControl.GiantPattern.StoneStomp:
                projectile = giantAI.dripStoneProjectiles[Random.Range(0, giantAI.dripStoneProjectiles.Length)];
                break;
            case BossGiantAttackControl.GiantPattern.Smash:
                projectile = giantAI.stoneProjectiles[Random.Range(0, giantAI.stoneProjectiles.Length)];
                break;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (giantAI.isPatternUsing && giantAI.patternCur == pattern && !giantMove.isMove)
        {
            //giantAI.isPatternActive = true;
            switch (pattern)
            {
                case BossGiantAttackControl.GiantPattern.Swing: //휘두르기
                    if (other.CompareTag(Data_Strings.playerTag))
                    {
                        other.gameObject.GetComponent<UnitStatus>().TakeDamage(DamagePayLoad());
                    }
                    break;
                case BossGiantAttackControl.GiantPattern.Smash: //돌떨구기 + 충격파
                    if (other.CompareTag(Data_Strings.terrainTag))
                    {
                        SmashPattern();
                    }
                    break;
                case BossGiantAttackControl.GiantPattern.RStomp: //짤몹소환
                    if (other.CompareTag(Data_Strings.terrainTag))
                    {
                        StompPattern();
                    }
                    break;
                case BossGiantAttackControl.GiantPattern.StoneStomp: //종유석
                    if (other.CompareTag(Data_Strings.terrainTag))
                    {
                        DripStonePattern();
                    }
                    break;
                default:
                    break;
            }
        }
    }


    DamagePayload DamagePayLoad()
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

    void SmashPattern()
    {
        StartCoroutine(StoneFire());
        ShockExplode(DamagePayLoad());
    }

    void StompPattern()
    {
        StartCoroutine(EnemyGenerate());
        ShockExplode(DamagePayLoad());
    }

    void DripStonePattern()
    {
        StartCoroutine(StoneFire());
    }


    IEnumerator StoneFire()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < attackCount; i++)
        {
            float randX = Random.Range(-projectileRandRange, projectileRandRange);
            Vector3 _randPos = new Vector3(target.position.x, 14.5f, 0f);
            _randPos.x += randX;

            GameObject _bullet = Instantiate(projectile, _randPos, Quaternion.identity);
            _bullet.GetComponent<Bullet>().SetBulletStatus(
                (int)(unitStat.atkCur * damageRevision),
                50f,
                0f,
                WeaponParamsSO.AtkTypes.Normal,
                false,
                0f,
                0f,
                unitStat
                );

            yield return new WaitForSeconds(projectileFireDelay);
        }

    }

    IEnumerator EnemyGenerate()
    {
        yield return new WaitForSeconds(0.8f);
        for (int i = 0; i < attackCount; i++)
        {
            Transform[] _positions = giantAI.mobSpawnPoints;
            Shuffle<Transform>(_positions);

            Vector3 _spawnPos = _positions[i].position;
            GameObject _unit = giantAI.spawnMobs[Random.Range(0, giantAI.spawnMobs.Length)];

            Instantiate(_unit, _spawnPos, Quaternion.identity, stageConTr);
            yield return new WaitForSeconds(projectileFireDelay);
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

    void ShockExplode(DamagePayload payload)
    {

        // 폭발 이펙트 생성
        if (shockEft != null)
        {
            GameObject eft = Instantiate(shockEft, tr.position, Quaternion.identity);
            eft.transform.localScale = Vector3.one * shockRadius;
        }

        // 범위 내에 있는 대상 탐색
        Collider[] hitColliders = Physics.OverlapSphere(tr.position, shockRadius, damageableLayers);
        float sqrExplosionRadius = shockRadius * shockRadius;

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


