using UnityEngine;
using System.Collections;
using static Michsky.UI.Heat.GradientFilter;
using static UnityEngine.ParticleSystem;

public class EnemyAttackSystem : MonoBehaviour
{
    public UnitAIParamsSO unitAIDataSource;

    public UnitAIParams aiParams { get; private set; }
    public UnitAIParams aiParamsDefault { get; private set; }

    [Header("WeaponProp")]
    [SerializeField] GameObject weaponProp;
    [SerializeField] Transform[] rotateTr;
    [SerializeField] Transform handTr;
    EnemyWeapon_Trasnforms weaponTrInfo;
    public GameObject bulletObj { get; private set; }


    [Header("WeaponStats")]
    [SerializeField] float w_accuracy;
    public int w_atk { get; private set; }
    public float w_range { get; private set; }
    UnitStatus thisStat;

    public AttackType atkType;
    public enum AttackType { Melee, Gunner, Sniper, Grenadier, Atillery, Bomber }


    int attackCount;
    float attackCoolTime;
    [SerializeField] float attackCoolTime_randRange;
    float attackDelay;
    WaitForSeconds _attackDelay;
    public float sightRange { get; private set; }

    [SerializeField] Transform frontOfUnit;
    [SerializeField] LayerMask hitLayers; // 감지할 레이어 지정

    bool isCombatStarted = false;


    public enum Ammos
    {
        Default,
        Infantry,
        Armor
    }


    public enum AtkArmor
    {
        Normal,
        Piercing_Light,
        Piercing_Heavy,
        Fixed
    }

    Ammos dropAmmo;
    [SerializeField] AtkArmor attackArmor;

    string[] weaponTriggerStrs = { "Weapon_Knife", "Weapon_Sword", "Weapon_Pistol", "Weapon_Rifle", "Weapon_Rocket", "Weapon_Grenade" };



    [Header("System")]
    Animator aniCon;
    public bool isEngage { get; private set; } = false;
    ObjectPool_Enemy bulletPool;
    Transform fireTr;
    public Transform target { get; private set; }
    float bullet_angleError = 3f;
    public bool isDead { get; set; } = false;
    [SerializeField] GameObject lineObj;
    EnemyWeapon_Grenade grenadeInfo;


    void EnemyWeaponEquip()
    {
        GameObject _weapon = Instantiate(weaponProp);
        weaponProp = _weapon;
        weaponTrInfo = _weapon.GetComponent<EnemyWeapon_Trasnforms>();

        switch (weaponTrInfo.weaponAniType)
        {
            case EnemyWeapon_Trasnforms.WeaponTypes.Knife:
            case EnemyWeapon_Trasnforms.WeaponTypes.Sword:
                _weapon.transform.SetParent(handTr, false);
                atkType = AttackType.Melee;
                dropAmmo = Ammos.Infantry;
                break;
            case EnemyWeapon_Trasnforms.WeaponTypes.Pistol:
                SetShooterType(0, _weapon);
                break;
            case EnemyWeapon_Trasnforms.WeaponTypes.Rifle:
                SetShooterType(1, _weapon);
                break;
            case EnemyWeapon_Trasnforms.WeaponTypes.Rocket:
                SetShooterType(2, _weapon);
                break;
            case EnemyWeapon_Trasnforms.WeaponTypes.Grenade:
                _weapon.transform.SetParent(handTr, false);
                atkType = AttackType.Grenadier;
                break;
        }

        

        switch (atkType)
        {
            case AttackType.Melee:
            case AttackType.Gunner:
                dropAmmo = Ammos.Infantry;
                break;

            case AttackType.Grenadier:
            case AttackType.Sniper:
            case AttackType.Atillery:
            case AttackType.Bomber:
                dropAmmo = Ammos.Armor;
                break;
            default:
                dropAmmo = Ammos.Infantry;
                break;
        }

        if(weaponTrInfo.bulletPoint != null)
        {
            fireTr = weaponTrInfo.bulletPoint;
        }

        if(weaponTrInfo.bulletObj != null)
        {
            bulletObj = weaponTrInfo.bulletObj;
        }
        
    }

    void SetShooterType(int type, GameObject weapon)
    {
        if (atkType == AttackType.Melee)
        {
            atkType = AttackType.Gunner;
        }
        weapon.transform.SetParent(rotateTr[type], false);
    }

    private void Awake()
    {
        EnemyWeaponEquip();
        AIStatusSet();


        aniCon = GetComponent<Animator>();

        aniCon.SetTrigger(weaponTriggerStrs[(int)weaponTrInfo.weaponAniType]);
        bulletPool = GetComponent<ObjectPool_Enemy>();
    }

    void AIStatusSet()
    {
        aiParams = new UnitAIParams(unitAIDataSource);
        aiParamsDefault = new UnitAIParams(aiParams); // 백업용 복사

        attackCount = aiParams.ai_atkCount;
        attackCoolTime = aiParams.ai_atkSpeed;
        attackDelay = aiParams.ai_atkDelay;
        w_range = aiParams.ai_atkRange;
        sightRange = aiParams.ai_sightRange;

        _attackDelay = new WaitForSeconds(attackDelay);
    }


    private void Start()
    {
        thisStat = GetComponent<UnitStatus>();
        w_atk = thisStat.unitParamsDefault.u_atk + w_atk;

        float _accError = Mathf.Lerp(4.75f, 0f, Mathf.Clamp01(w_accuracy * 0.01f));

        // 사거리 기반 오차 (50일 때 0도, 5일 때 2.75도)
        float rangeNormalized = Mathf.Clamp01(Mathf.InverseLerp(5f, 50f, w_range));
        float _rangeError = Mathf.Lerp(2.75f, 0f, rangeNormalized);

        // 총 오차 (최대 7.5도 제한. -7.5 ~ +7.5 범위이므로 실제 적용 값은 최대 15도)
        bullet_angleError = Mathf.Clamp(_accError + _rangeError, 0f, 7.5f);

    }

    public void UnitCombat(bool _isInRange, Transform _target)
    {
        target = _target;
        isEngage = _isInRange;
        if (_isInRange && !isCombatStarted)
        {
            isCombatStarted = true;

            switch (atkType)
            {
                case AttackType.Melee:
                    StartCoroutine(AttackMelee());
                    break;
                case AttackType.Gunner:
                    StartCoroutine(AttackGun());
                    break;
                case AttackType.Sniper:
                    StartCoroutine(AttackSniper());
                    break;
                case AttackType.Grenadier:
                    StartCoroutine(AttackGrenadier());
                    break;
                case AttackType.Bomber:
                    StartCoroutine(AttackBomber());
                    break;
                default:
                    Debug.Log("공격패턴 오류");
                    break;
            }

        }


    }

    IEnumerator AttackGun()
    {
        while (isEngage)
        {
            for(int i = 0; i < attackCount; i++)
            {
                WeaponShoot();
                yield return _attackDelay;
            }
            float coolTime = Random.Range(attackCoolTime - attackCoolTime_randRange, attackCoolTime + attackCoolTime_randRange);
            CheckTargetDead();

            yield return new WaitForSeconds(coolTime);
        }
        isCombatStarted = false;
    }

    IEnumerator AttackMelee()
    {
        while (isEngage)
        {
            aniCon.SetTrigger("Attack");

            yield return _attackDelay;

            WeaponSlash();
            CheckTargetDead();

            float coolTime = Random.Range(attackCoolTime - attackCoolTime_randRange, attackCoolTime + attackCoolTime_randRange);
            yield return new WaitForSeconds(coolTime);
        }
        isCombatStarted = false;
    }

    IEnumerator AttackSniper()
    {
        Enemy_AimingLine line = lineObj.GetComponent<Enemy_AimingLine>();
        while (isEngage && !gameObject.CompareTag("Dead"))
        {
            lineObj.SetActive(true);
            line.SetTransforms(target, fireTr);
            line.isLineDraw = true;
            for (int i = 0; i < 10;)
            {
                Vector3 dir = (target.position + (Vector3.up * 1.25f) - fireTr.position).normalized;
                if (CheckPlayerHit(fireTr.position, dir) != null)
                {
                    i++;
                    lineObj.SetActive(true);
                }
                else
                {
                    i = 0;
                    lineObj.SetActive(false);
                }
                yield return new WaitForSeconds(0.075f);
            }

            line.Blink(true, isDead);

            yield return new WaitForSeconds(0.75f);

            line.Blink(false, isDead);
            WeaponShoot(line.PinPoint());
            line.isLineDraw = false;
            lineObj.SetActive(false);
            float coolTime = Random.Range(attackCoolTime - attackCoolTime_randRange, attackCoolTime + attackCoolTime_randRange);
            CheckTargetDead();
            yield return new WaitForSeconds(coolTime);
        }
        isCombatStarted = false;
    }

    IEnumerator AttackGrenadier()
    {
        while (isEngage)
        {
            Vector3 dir = (target.position + (Vector3.up * 1.25f) - frontOfUnit.position).normalized;
            if (CheckPlayerHit(frontOfUnit.position, dir) != null)
            {
                WeaponBombShoot();
            }
            float coolTime = Random.Range(attackCoolTime - attackCoolTime_randRange, attackCoolTime + attackCoolTime_randRange);
            CheckTargetDead();

            yield return new WaitForSeconds(coolTime);
        }
        isCombatStarted = false;
    }

    IEnumerator AttackBomber()
    {
        while (isEngage)
        {
            for (int i = 0; i < attackCount; i++)
            {
                BombShoot(target.position);
                yield return _attackDelay;
            }
            float coolTime = Random.Range(attackCoolTime - attackCoolTime_randRange, attackCoolTime + attackCoolTime_randRange);
            CheckTargetDead();

            yield return new WaitForSeconds(coolTime);
        }
        isCombatStarted = false;
    }

    void WeaponShoot()
    {
        if(target != null && !isDead)
        {
            aniCon.SetTrigger("Attack");

            GameObject bullet = bulletPool.GetObject();
            bullet.GetComponent<Bullet>().SetBulletStatus(w_atk, w_range);
            Transform bulletTr = bullet.transform;
            if(fireTr != null)
            {
                bulletTr.position = new Vector3(fireTr.position.x, fireTr.position.y, 0f);
            }
            else
            {
                bulletTr.position = new Vector3(transform.position.x, transform.position.y, 0f);
            }
            

            // 방향 설정
            Vector3 direction = (target.position + (Vector3.up * 1.25f) - bulletTr.position).normalized;
            Quaternion baseRotation = Quaternion.LookRotation(direction);

            // X축 회전 수정
            float _angleError = Random.Range(-bullet_angleError, bullet_angleError);
            Vector3 eulerAngles = baseRotation.eulerAngles;
            eulerAngles.x += Random.Range(-_angleError, _angleError); // X축에 _accuracy 값 추가
            bulletTr.rotation = Quaternion.Euler(eulerAngles);

        }

    }

    void WeaponShoot(Vector3 _position)
    {
        if (target != null && !isDead)
        {
            aniCon.SetTrigger("Attack");

            GameObject bullet = bulletPool.GetObject();
            bullet.GetComponent<Bullet>().SetBulletStatus(w_atk, w_range);
            Transform bulletTr = bullet.transform;
            if (fireTr != null)
            {
                bulletTr.position = new Vector3(fireTr.position.x, fireTr.position.y, 0f);
            }
            else
            {
                bulletTr.position = new Vector3(transform.position.x, transform.position.y, 0f);
            }


            // 방향 설정
            Vector3 direction = (_position + (Vector3.up * 1.25f) - bulletTr.position).normalized;
            Quaternion baseRotation = Quaternion.LookRotation(direction);

            // X축 회전 수정
            float _angleError = Random.Range(-bullet_angleError, bullet_angleError);
            Vector3 eulerAngles = baseRotation.eulerAngles;
            eulerAngles.x += Random.Range(-_angleError, _angleError); // X축에 _accuracy 값 추가
            bulletTr.rotation = Quaternion.Euler(eulerAngles);

        }

    }

    void BombShoot(Vector3 _position)
    {
        if (target != null && !isDead)
        {
            // X축 위치 수정
            float _posError = Random.Range(-bullet_angleError, bullet_angleError) * 1.25f;

            GameObject bomb = bulletPool.GetObject();
            bomb.GetComponent<Bullet>().SetBulletStatus(w_atk, w_range);
            Transform bulletTr = bomb.transform;
            bulletTr.position = new Vector3(target.position.x + _posError, target.position.y + 30f, 0f);
            bulletTr.LookAt(bulletTr.position + Vector3.down);

        }

    }

    void WeaponBombShoot()
    {
        if (target != null && !isDead)
        {
            aniCon.SetTrigger("Attack");
        }
    }



    void WeaponSlash()
    {
        UnitStatus hitUnitStat = CheckPlayerHit(frontOfUnit.position, frontOfUnit.forward);
        Debug.Log(hitUnitStat);
        if (hitUnitStat != null)
        {
            hitUnitStat.UnitGetDamage(w_atk, 0, (int)attackArmor);
        }
    }

    UnitStatus CheckPlayerHit(Vector3 origin, Vector3 direction) // Ray로 플레이어 감지 여부 확인
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, w_range, hitLayers))
        {
            Debug.DrawLine(origin, hit.point, Color.yellow, 1.0f);
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                return hit.collider.gameObject.GetComponent<UnitStatus>();
            }
        }

        return null;
    }

    public void SetWeaponPropToHand()
    {
        weaponProp.transform.SetParent(handTr);
    }

    void CheckTargetDead()
    {
        if (target.gameObject.CompareTag("Dead"))
        {
            isEngage = false;
            GetComponent<EnemyUnitAI_Controller>().StateChange = EnemyUnitAI_Controller.UnitState.Return;
        }
    }

    public void SetGrenadeComponent(EnemyWeapon_Grenade comp)
    {
        grenadeInfo = comp;
        grenadeInfo.unitAnimator = aniCon;
    }


}
