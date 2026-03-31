using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using static Michsky.UI.Heat.GradientFilter;

public class BossBomberNormalWeapon : MonoBehaviour
{
    BossControlSystem bossStatus;
    UnitStatus stat;
    BossBomberLookPlayer thisAI;

    [SerializeField] GameObject bulletObj;
    [SerializeField] Transform[] bulletPoints;

    [SerializeField] int attackCount = 1;
    [SerializeField] float attackDelay = 1f;
    [SerializeField] float attackCooldown = 1f;
    Transform target;

    [SerializeField] float damageRevision = 1f;

    int w_atk = 0;
    [SerializeField] float w_range = 50f;
    [SerializeField] float w_accuracy = 0f;

    float bullet_angleError = 3f;

    int bulletPoint = 0;

    AudioSource soundPlayer;
    [SerializeField] AudioClip[] sounds_WeaponShot;
    [SerializeField] AudioClip sound_BonusShot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bossStatus = GetComponentInParent<BossControlSystem>();
        thisAI = GetComponentInParent<BossBomberLookPlayer>();
        stat = GetComponentInParent<UnitStatus>();
        soundPlayer = GetComponentInParent<AudioSource>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(GunShoot());
        SetWeaponStat();
    }

    void SetWeaponStat()
    {
        w_atk = Mathf.RoundToInt(stat.unitParams.u_atk * damageRevision);
        
        
        float _accError = Mathf.Lerp(4.75f, 0f, Mathf.Clamp01(w_accuracy * 0.01f));

        // 사거리 기반 오차 (50일 때 0도, 5일 때 2.75도)
        float rangeNormalized = Mathf.Clamp01(Mathf.InverseLerp(5f, 50f, 5f));
        float _rangeError = Mathf.Lerp(2.75f, 0f, rangeNormalized);
        // 총 오차 (최대 7.5도 제한. -7.5 ~ +7.5 범위이므로 실제 적용 값은 최대 15도)
        bullet_angleError = Mathf.Clamp(_accError + _rangeError, 0f, 7.5f);
    }

    IEnumerator GunShoot()
    {
        yield return new WaitForSeconds(3f);

        while (bossStatus.isBossLive)
        {
            if (thisAI.normalAttackCount > thisAI.normalAttackCountCur && !thisAI.isPatternUsing && Vector3.Distance(transform.position, target.position) <= w_range)
            {
                thisAI.normalAttackCountCur++;
                for (int i = 0; i < attackCount; i++)
                {
                    WeaponShoot();
                    yield return new WaitForSeconds(attackDelay);
                }
                
            }

            yield return new WaitForSeconds(attackCooldown);
        }
    }


    void WeaponShoot()
    {
        if (target != null && bossStatus.isBossLive)
        {
            bulletPoint = 1 - bulletPoint;
            GameObject bullet = Instantiate(bulletObj, bulletPoints[bulletPoint].position, Quaternion.identity);

            
            Transform bulletTr = bullet.transform;

            
            bulletTr.position = bulletPoints[bulletPoint].position;


            // 방향 설정
            Vector3 direction = (target.position + (Vector3.up * 1.25f) - bulletTr.position).normalized;
            Quaternion baseRotation = Quaternion.LookRotation(direction);

            // X축 회전 수정
            float _angleError = Random.Range(-bullet_angleError, bullet_angleError);
            Vector3 eulerAngles = baseRotation.eulerAngles;
            eulerAngles.x += Random.Range(-_angleError, _angleError); // X축에 _accuracy 값 추가
            bulletTr.rotation = Quaternion.Euler(eulerAngles);


            if (bullet.GetComponent<Bullet>() == null)
            {
                ShootSoundPlay(true);
                bullet.GetComponent<Projectile_Bomb>().SetBulletStatus(w_atk, w_range);
                bullet.GetComponent<Projectile_Bomb>().ForceShoot();
                Destroy(bullet, 10f);
            }
            else
            {
                ShootSoundPlay(false);
                bullet.GetComponent<Bullet>().SetBulletStatus(w_atk, w_range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 0f, 0f, stat);
                Destroy(bullet, 5f);
            }
        }
    }

    void ShootSoundPlay(bool isGrenade)
    {
        if (soundPlayer != null)
        {
            AudioClip _clip = sound_BonusShot;
            if (!isGrenade)
            {
                int _randValue = Random.Range(0, sounds_WeaponShot.Length);
                _clip = sounds_WeaponShot[_randValue];
            }
            soundPlayer.PlayOneShot(_clip);
        }
    }


}
