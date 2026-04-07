using UnityEngine;
using System.Collections;
using Combat;

public class Direction_Tutorial_MissileShoot : MonoBehaviour
{
    [SerializeField] GameObject missilePrefab;

    [SerializeField] Transform[] targets;

    [SerializeField] DamagePayload dmg;

    [SerializeField] UnitStatus[] enemies;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Missile());
    }


    void ShootMissile(Transform target)
    {
        Vector3 _fireTr = target.position + (Vector3.up * 30f);

        GameObject bullet = Instantiate(missilePrefab, _fireTr, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetBulletStatus(5000, 25000f, 40f);
        Transform bulletTr = bullet.transform;

        // 방향 설정
        //Vector3 direction = (playerPointer.targetPos - bulletTr.position).normalized;
        Vector3 direction = (target.position - _fireTr).normalized;
        Quaternion baseRotation = Quaternion.LookRotation(direction);

        // X축 회전 수정
        float _angleError = Random.Range(-1f, 1f);
        Vector3 eulerAngles = baseRotation.eulerAngles;
        eulerAngles.x += Random.Range(-_angleError, _angleError); // X축에 _accuracy 값 추가
        bulletTr.rotation = Quaternion.Euler(eulerAngles);



    }

    IEnumerator Missile()
    {
        yield return new WaitForSeconds(3f);

        ShuffleTargets(targets); // ← 여기서 섞는다

        for (int i = 0; i < targets.Length; i++)
        {
            ShootMissile(targets[i]);

            float randomDelay = Random.Range(0.08f, 0.3f);
            yield return new WaitForSeconds(randomDelay);
        }

        for(int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].hpCur > 0)
            {
                DamagePayload _dmg = dmg;
                _dmg.hitPoint = enemies[i].transform.position + Vector3.up;
                enemies[i].TakeDamage(_dmg);
            }
            
        }
    }

    void ShuffleTargets(Transform[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            // swap
            Transform temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}
