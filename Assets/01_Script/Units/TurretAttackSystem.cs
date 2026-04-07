using UnityEngine;
using System.Collections;

public class TurretAttackSystem : MonoBehaviour
{

    public UnitAIParamsSO unitAIDataSource;

    public UnitAIParams aiParams { get; private set; }
    public UnitAIParams aiParamsDefault { get; private set; }


    [Header("WeaponProp")]
    public GameObject bulletObj;
    [SerializeField] Transform gunTr;
    [SerializeField] Transform returnTr;


    [Header("WeaponStats")]
    [SerializeField] float w_accuracy;
    public int w_atk { get; private set; }
    public float w_range { get; private set; }
    UnitStatus thisStat;
    EnemyTurret turretSystem;


    int attackCount;
    float attackCoolTime;
    [SerializeField] float attackCoolTime_randRange;
    float attackDelay;
    WaitForSeconds _attackDelay;
    public float sightRange { get; private set; }

    [SerializeField] LayerMask hitLayers; // 감지할 레이어 지정



    [Header("System")]
    bool isEngageReady = true;
    public bool isEngage { get; private set; } = false;
    ObjectPool_Enemy bulletPool;
    [SerializeField] Transform fireTr;
    public Transform target { get; private set; }
    float bullet_angleError = 3f;
    public bool isDead { get; set; } = false;

    AudioSource w_soundPlayer;
    [SerializeField] AudioClip[] w_soundsFire;

    [SerializeField] private float rotationSpeed = 90f; // 초당 회전 속도 (deg/sec)
    Coroutine rotateRoutine;

    public bool isPlayerInRange { get; set; } = false;


    private void Awake()
    {
        AIStatusSet();
        bulletPool = GetComponent<ObjectPool_Enemy>();
        turretSystem = GetComponent<EnemyTurret>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisStat = GetComponent<UnitStatus>();
        w_atk = thisStat.unitParamsDefault.u_atk + w_atk;

        float _accError = Mathf.Lerp(4.75f, 0f, Mathf.Clamp01(w_accuracy * 0.01f));

        // 사거리 기반 오차 (50일 때 0도, 5일 때 2.75도)
        float rangeNormalized = Mathf.Clamp01(Mathf.InverseLerp(5f, 50f, w_range));
        float _rangeError = Mathf.Lerp(2.75f, 0f, rangeNormalized);

        // 총 오차 (최대 7.5도 제한. -7.5 ~ +7.5 범위이므로 실제 적용 값은 최대 15도)
        bullet_angleError = Mathf.Clamp(_accError + _rangeError, 0f, 7.5f);

        w_soundPlayer = GetComponent<AudioSource>();

        StartCoroutine(AI_Action());
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


    /// <summary>
    /// 외부에서 목표 방향(월드 위치)을 지정해 회전 시작
    /// </summary>
    void RotateTo(Vector3 targetPos)
    {
        if (!isDead)
        {
            if (rotateRoutine != null)
                StopCoroutine(rotateRoutine);

            rotateRoutine = StartCoroutine(RotateSmoothly(targetPos));
        }
    }

    IEnumerator RotateSmoothly(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - gunTr.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        // 중력 반전 시 Z축 회전값 반대로  
        if (turretSystem.isGravityReverse)
        {
            // Z축 180도 회전 추가
            targetRot *= Quaternion.Euler(0f, 0f, 180f);
        }

        while (Quaternion.Angle(gunTr.rotation, targetRot) > 0.1f)
        {
            gunTr.rotation = Quaternion.RotateTowards(
                gunTr.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        gunTr.rotation = targetRot;
        rotateRoutine = null;
    }

    public void PlayerApproach(bool isApproach)
    {
        if(target == null)
        {
            target = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).transform;
        }

        DroneMoveSystem _drone = GetComponent<DroneMoveSystem>();
        {
            if(_drone != null)
            {
                _drone.SetTarget(target);
            }
        }

        isPlayerInRange = isApproach;
    }

    IEnumerator AI_Action()
    {
        while (true)
        {
            if (CheckPlayerInRange())
            {
                StartEngage(true, target);
            }


            yield return new WaitForSeconds(0.5f);
        }
    }


    bool CheckPlayerInRange()
    {
        if (target == null) return false;

        Vector3 eyeToTarget = (target.position + Vector3.up * 1.3f) - gunTr.position;
        Vector3 direction = eyeToTarget.normalized;

        float sqrDistance = (target.position - transform.position).sqrMagnitude;
        float minDistanceSqr = 1f * 1f; // 보완용 최소 거리 (조절 가능)

        // 1. Ray 감지 우선
        if (Physics.Raycast(gunTr.position, direction, out RaycastHit hit, w_range, hitLayers))
        {
            Debug.DrawLine(gunTr.position, hit.point, Color.blue, 1.0f);
            if (hit.transform == target)
                return true;
        }

        // 2. 너무 가까우면 강제 감지 성공 처리
        if (sqrDistance < minDistanceSqr)
        {
            Debug.DrawLine(gunTr.position, target.position, Color.green, 1.0f);
            return true;
        }

        return false;
    }


    void StartEngage(bool isInRange, Transform attackTarget)
    {
        if(isInRange && isEngageReady)
        {
            isEngage = true;
            isEngageReady = false;
            StartCoroutine(AttackGun());
        }
        RotateTo(target.position + Vector3.up);
    }

    IEnumerator AttackGun()
    {
        while (isEngage && !isDead)
        {
            for (int i = 0; i < attackCount; i++)
            {
                WeaponShoot();
                yield return _attackDelay;
            }
            float coolTime = Random.Range(attackCoolTime - attackCoolTime_randRange, attackCoolTime + attackCoolTime_randRange);
            CheckTargetDead();
            isEngage = CheckPlayerInRange();

            yield return new WaitForSeconds(coolTime);
        }
        isEngageReady = true;
        RotateTo(returnTr.position);
    }


    void WeaponShoot()
    {
        if (target != null && !isDead)
        {

            GameObject bullet = bulletPool.GetObject();
            bullet.GetComponent<Bullet>().SetBulletStatus(w_atk, w_range, 0f, WeaponParamsSO.AtkTypes.Normal, false, 0f, 0f, thisStat);
            Transform bulletTr = bullet.transform;
            if (fireTr != null)
            {
                bulletTr.position = fireTr.position; //new Vector3(fireTr.position.x, fireTr.position.y, 0f);
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

            PlaySoundFire();
        }

    }

    void PlaySoundFire()
    {
        int _randValue = Random.Range(0, w_soundsFire.Length);

        w_soundPlayer.PlayOneShot(w_soundsFire[_randValue]);
    }

    void CheckTargetDead()
    {
        if (target.gameObject.CompareTag("Dead"))
        {
            isEngage = false;

        }
    }
}
