using UnityEngine;
using System.Collections;

public class BossBomberSpecialWeapon : MonoBehaviour
{
    public BossBomberLookPlayer.BossMoveType weaponType;
    BossBomberLookPlayer thisAI;
    BossControlSystem bossStatus;
    UnitStatus stat;

    [SerializeField] GameObject bulletObj;

    [SerializeField] Transform bulletPoint;
    [SerializeField] Transform gatling_BulletLook;


    [SerializeField] int attackCount = 1;
    [SerializeField] float attackDelay = 1f;
    [SerializeField] float attackCooldown = 1f;

    [SerializeField] float damageRevision = 1f;

    int w_atk = 0;
    [SerializeField] float w_range = 50f;
    [SerializeField] float w_accuracy = 0f;

    float bullet_angleError = 3f;

    Transform target;

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
        StartShootingSystem();
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

    void StartShootingSystem()
    {
        switch (weaponType)
        {
            case BossBomberLookPlayer.BossMoveType.MissileR:
                StartCoroutine(MissileShoot());
                break;
            case BossBomberLookPlayer.BossMoveType.MissileL:
                StartCoroutine(MissileShoot());
                break;
            case BossBomberLookPlayer.BossMoveType.Gatling:
                StartCoroutine(GatlingShoot());
                break;
            case BossBomberLookPlayer.BossMoveType.Airborne:
                break;
            case BossBomberLookPlayer.BossMoveType.Charge:
                StartCoroutine (ChargeShoot());
                break;
            default:
                break;
        }
    }

    IEnumerator GatlingShoot()
    {
        while (bossStatus.isBossLive)
        {
            if (thisAI.isAttackReady && weaponType == thisAI.attackPattern)
            {
                WeaponLinearShoot();
            }

            yield return new WaitForSeconds(attackDelay);
        }
    }


    IEnumerator MissileShoot()
    {
        while (bossStatus.isBossLive)
        {
            if (thisAI.isAttackReady && weaponType == thisAI.attackPattern)
            {
                WeaponLinearShoot();
                thisAI.MissileShoot();
                thisAI.isAttackReady = false;
                thisAI.patternAttackCountCur++;
            }

            yield return new WaitForSeconds(attackCooldown);
        }
    }

    IEnumerator ChargeShoot()
    {
        while (bossStatus.isBossLive)
        {
            if (thisAI.isAttackReady && weaponType == thisAI.attackPattern)
            {
                for (int i = 0; i < attackCount; i++)
                {
                    WeaponSpreadShoot();
                    yield return new WaitForSeconds(attackDelay);
                }
                thisAI.patternAttackCountCur++;
            }

            yield return new WaitForSeconds(attackCooldown);
        }
    }

    void WeaponLinearShoot()
    {
        if (target != null)
        {
            GameObject bullet = Instantiate(bulletObj, bulletPoint.position, Quaternion.identity);

            Transform bulletTr = bullet.transform;

            bulletTr.position = bulletPoint.position;
            
            if(gatling_BulletLook == null)
            {
                bulletTr.LookAt(bulletPoint.forward);
            }
            else
            {
                bulletTr.LookAt(gatling_BulletLook);
            }

            ShootSoundPlay();

            bullet.GetComponent<Bullet>().SetBulletStatus(w_atk, w_range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 2f, 0f, stat);
            Destroy(bullet, 5f);
        }
    }

    void WeaponSpreadShoot()
    {

        if (target != null)
        {
            GameObject bullet = Instantiate(bulletObj, bulletPoint.position, Quaternion.identity);


            Transform bulletTr = bullet.transform;


            bulletTr.position = bulletPoint.position;


            // 방향 설정
            Vector3 direction = (target.position + (Vector3.up * 1.25f) - bulletTr.position).normalized;
            Quaternion baseRotation = Quaternion.LookRotation(direction);

            // X축 회전 수정
            float _angleError = Random.Range(-bullet_angleError, bullet_angleError);
            Vector3 eulerAngles = baseRotation.eulerAngles;
            eulerAngles.x += Random.Range(-_angleError, _angleError); // X축에 _accuracy 값 추가
            bulletTr.rotation = Quaternion.Euler(eulerAngles);

            ShootSoundPlay();

            bullet.GetComponent<Bullet>().SetBulletStatus(w_atk, w_range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 0f, 0f, stat);
            Destroy(bullet, 5f);
        }
    }

    void ShootSoundPlay()
    {
        if (soundPlayer != null)
        {
            int _randValue = Random.Range(0, sounds_WeaponShot.Length);
            AudioClip _clip = sounds_WeaponShot[_randValue];
            soundPlayer.PlayOneShot(_clip);
            if (sound_BonusShot != null)
            {
                soundPlayer.PlayOneShot(sound_BonusShot);
            }
        }
    }
}
