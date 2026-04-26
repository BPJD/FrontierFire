using UnityEngine;
using System.Collections;

public class BossCoreAttackControl : MonoBehaviour
{
    UnitStatus bossStat;
    [SerializeField] float atkRevision = 1f;
    int w_atk = 0;
    [SerializeField] float w_range = 50f;

    [SerializeField] Transform[] bulletPos;     // 투사체 목표 지점 (5개)
    [SerializeField] GameObject bulletPrefab;   // 생성할 투사체 프리팹
    [SerializeField] float moveSpeed = 2f;      // 이동 속도

    public bool isAttackReady = false;

    Transform attackTarget;


    [SerializeField] float attackCoolDown = 5f;
    float cooldownCur = 0f;


    void Start()
    {
        bossStat = GetComponent<UnitStatus>();
        w_atk = (int)(bossStat.unitParams.u_atk * atkRevision);
        attackTarget = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        cooldownCur -= Time.deltaTime;

        if (bossStat.hpCur > 0 && isAttackReady && cooldownCur < 0f)
        {
            SpawnAndMoveBullets();
            cooldownCur = attackCoolDown;
        }
    }


    void SpawnAndMoveBullets()
    {
        // bulletPos 갯수만큼 투사체 생성
        for (int i = 0; i < bulletPos.Length; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

            Bullet bulletStat = bullet.GetComponent<Bullet>();
            bulletStat.SetBulletStatus(w_atk, w_range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 0f, 0f, bossStat);
            bulletStat.isMove = false;
            float randSpd = Random.Range(moveSpeed * 0.7f, moveSpeed * 1.3f);
            StartCoroutine(MoveBulletToPosition(bullet, bulletPos[i], bulletStat, randSpd));
            Destroy(bullet, 10f);
        }
    }

    IEnumerator MoveBulletToPosition(GameObject bullet, Transform target, Bullet stat, float speed)
    {
        Transform bulletTr = bullet.transform;
        while (bullet != null && Vector3.Distance(bulletTr.position, target.position) > 0.05f)
        {
            bulletTr.position = Vector3.MoveTowards(
                bulletTr.position,
                target.position,
                speed * Time.deltaTime
            );

            bulletTr.LookAt(attackTarget.position + Vector3.up);
            
            yield return null;
        }

        // 목표 지점 도착 후 → 남길지/없앨지 결정
        if (bullet != null)
        {
            stat.isMove = true;
            // Destroy(bullet); // 필요하다면 제거
        }
    }
}