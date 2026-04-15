using Combat;
using System.Collections;
using UnityEngine;
using static Michsky.UI.Heat.GradientFilter;

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
        _fireTr.z = 0f;

        GameObject bullet = Instantiate(missilePrefab, _fireTr, Quaternion.identity);

        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.SetBulletStatus(5000, 25000f, 40f);
        }

        Transform bulletTr = bullet.transform;
        bulletTr.position = new Vector3(_fireTr.x, _fireTr.y, 0f);

        Vector3 targetPos = target.position;
        targetPos.z = 0f;

        Vector3 direction = (targetPos - bulletTr.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion baseRotation = Quaternion.LookRotation(direction);

            float angleError = Random.Range(-2.5f, 2.5f);
            Quaternion spreadRotation = Quaternion.Euler(new Vector3(0f, angleError, 0f));

            bulletTr.rotation = baseRotation * spreadRotation;
        }
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
