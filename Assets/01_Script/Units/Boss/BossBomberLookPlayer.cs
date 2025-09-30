using UnityEngine;
using System.Collections;

public class BossBomberLookPlayer : MonoBehaviour
{
    public Animator aniCon {  get; private set; }

    BossBomberNormalWeapon normalWeapon;
    BossControlSystem bossStatus;

    Transform player;
    public float maxXAngle = 30f;
    public float rotationSpeed = 5f;

    public bool isMoving = false;
    public bool isLookingRight = false;
    bool isAngleSetted = false;

    public float moveSpeed = 20f;
    public float arriveThreshold = 0.05f;
    Coroutine moveRoutine;

    Vector3 positionCur;

    [SerializeField] Transform center1, center2, centerL, centerR, l1, l2, r1, r2, lEnd, rEnd;

    [SerializeField] Transform[] debug;

    Transform[] normalPoints;
    Transform[] endPoints;
    Transform[] rocketLPoints;
    Transform[] rocketRPoints;

    int startMissilePos = 0;

    public int normalAttackCount = 8;
    public int normalAttackCountCur = 0;

    public int patternAttackCount = 3;
    public int patternAttackCountCur = 0;
    public int patternStack = 0;

    Transform destination;
    bool isArrived = false;
    public BossMoveType attackPattern;
    public bool isAttackReady = false;
    public bool isPatternUsing { get; private set; } = false;
    bool isMissilePosition = false;



    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        bossStatus = GetComponent<BossControlSystem>();
        normalWeapon = GetComponentInChildren<BossBomberNormalWeapon>();
        normalPoints = new Transform[] { lEnd, centerL, center2, centerR, rEnd };
        endPoints = new Transform[] { lEnd, rEnd };
        rocketLPoints = new Transform[] { l1, l2 };
        rocketRPoints = new Transform[] { r1, r2 };

        aniCon = GetComponentInChildren<Animator>();

        StartCoroutine(PatternMove());
    }

    public enum BossMoveType
    {
        Normal,
        Gatling,
        Airborne,
        MissileL,
        MissileR,
        Charge,
        Pattern
    }

    IEnumerator PatternMove()
    {
        while (bossStatus.isBossLive)
        {
            float moveDelay = 0.5f;
            if (normalAttackCountCur < normalAttackCount)
            {
                aniCon.SetTrigger("Normal");
                switch (attackPattern)
                {
                    case BossMoveType.MissileL:
                        destination = centerL;
                        break;
                    case BossMoveType.MissileR:
                        destination = centerR;
                        break;
                    case BossMoveType.Charge:
                        destination = center2;
                        break;
                    default:
                        destination = normalPoints[Random.Range(0, normalPoints.Length)];
                        break;
                }
                attackPattern = BossMoveType.Normal;
                MoveTo(destination, BossMoveType.Normal);
                moveDelay = 3f;
            }
            else
            {
                if (isArrived || attackPattern == BossMoveType.Normal)
                {
                    switch (attackPattern)
                    {
                        case BossMoveType.Normal:
                            isPatternUsing = false;
                            attackPattern = SetPatternType();
                            MoveTo(destination, BossMoveType.Pattern);
                            break;
                        case BossMoveType.Pattern:
                            isPatternUsing = true;
                            MoveTo(destination, attackPattern);
                            break;
                        case BossMoveType.MissileL:
                            if (transform.position == destination.position && !isAttackReady)
                            {
                                destination = rocketLPoints[1 - startMissilePos];
                                startMissilePos = Mathf.Clamp(1 - startMissilePos, 0, 1);
                                isMissilePosition = true;
                                MoveTo(destination, BossMoveType.MissileL);
                            }
                            break;
                        case BossMoveType.MissileR:
                            if (transform.position == destination.position && !isAttackReady)
                            {
                                destination = rocketRPoints[1 - startMissilePos];
                                startMissilePos = Mathf.Clamp(1 - startMissilePos, 0, 1);
                                isMissilePosition = true;
                                MoveTo(destination, BossMoveType.MissileR);
                            }
                            break;
                        case BossMoveType.Gatling:
                            if (transform.position == destination.position)
                            {
                                int code = isLookingRight ? 0 : 1;
                                destination = endPoints[code];
                                MoveTo(destination, BossMoveType.Gatling);
                            }
                            break;
                        case BossMoveType.Airborne:
                            if (transform.position == destination.position)
                            {
                                int code = isLookingRight ? 0 : 1;
                                destination = endPoints[code];
                                MoveTo(destination, BossMoveType.Airborne);
                            }
                            break;
                        case BossMoveType.Charge:
                            if (transform.position == destination.position)
                            {
                                destination = center1;
                                MoveTo(destination, BossMoveType.Charge);
                            }
                            break;
                        default:
                            break;
                    }
                }
                
            }

            yield return new WaitForSeconds(moveDelay);
        }
    }

    BossMoveType SetPatternType()
    {
        if(patternStack <= 2)
        {
            int patternType = Random.Range(1, 5);
            attackPattern = (BossMoveType)patternType;
            int point = Random.Range(0, endPoints.Length);

            switch (attackPattern)
            {
                case BossMoveType.Gatling:
                    isLookingRight = point == 0 ? false : true;
                    isMoving = true;
                    destination = endPoints[point];
                    patternAttackCount = 2;
                    aniCon.SetTrigger("Gatling");
                    break;
                case BossMoveType.Airborne:
                    isMoving = true;
                    isLookingRight = point == 0 ? false : true;
                    destination = endPoints[point];
                    patternAttackCount = 3;
                    aniCon.SetTrigger("Idle");
                    break;
                case BossMoveType.MissileL:
                    destination = centerL;
                    isMoving = true;
                    isLookingRight = false;
                    patternAttackCount = 3;
                    aniCon.SetTrigger("Rocket");
                    break;
                case BossMoveType.MissileR:
                    destination = centerR;
                    isMoving = true;
                    isLookingRight = true;
                    patternAttackCount = 3;
                    aniCon.SetTrigger("Rocket");
                    break;
                default:
                    break;
            }

        }
        else
        {
            attackPattern = BossMoveType.Charge;
            destination = center2;
            isMoving = true;
            patternStack = 0;
            patternAttackCount = 5;
            aniCon.SetTrigger("Charge");
        }


            return attackPattern;
    }

    void Update()
    {
        if (bossStatus.isBossLive)
        {
            if (!isMoving)
            {
                Vector3 directionToPlayer = player.position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                Vector3 euler = smoothRotation.eulerAngles;
                euler.x = ClampAngle(euler.x, -maxXAngle, maxXAngle);

                transform.rotation = Quaternion.Euler(euler);
                isAngleSetted = false;
            }
            else
            {
                Vector3 euler = transform.rotation.eulerAngles;
                float targetX = 0f;
                float targetY = isLookingRight ? -90f : 90f;
                float targetZ = 0f;

                if (!isAngleSetted)
                {
                    targetX = euler.x;
                    isAngleSetted = true;
                }

                Quaternion targetRot = Quaternion.Euler(targetX, targetY, targetZ);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        angle = Mathf.Clamp(angle, min, max);
        return angle < 0f ? angle + 360f : angle;
    }

    public void MoveTo(Transform target, BossMoveType moveType)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        isArrived = false;

        switch (moveType)
        {
            case BossMoveType.Normal:
                float speed = Random.Range(2f, 5f);
                moveRoutine = StartCoroutine(MoveToTarget(target.position, speed, false, 0f));
                aniCon.SetBool("IsMove", false);
                isMoving = false;
                break;

            case BossMoveType.Gatling:
                moveRoutine = StartCoroutine(MoveToTarget(target.position, moveSpeed, false, 0f));
                aniCon.SetBool("IsMove", true);
                isMoving = true;

                break;

            case BossMoveType.MissileL:

                isMoving = true;
                moveRoutine = StartCoroutine(MoveToTarget(target.position, 0f, true, 0.3f));
                break;

            case BossMoveType.MissileR:
                isMoving = true;
                moveRoutine = StartCoroutine(MoveToTarget(target.position, 0f, true, 0.3f));
                break;

            case BossMoveType.Charge:
                moveRoutine = StartCoroutine(MoveToTarget(target.position, 0f, true, 1.5f));
                isMoving = true;
                break;

            case BossMoveType.Airborne:
                moveRoutine = StartCoroutine(MoveToTarget(target.position, moveSpeed * 0.5f, false, 0f));
                aniCon.SetBool("IsMove", true);
                isMoving = true;
                break;

            case BossMoveType.Pattern:
                moveRoutine = StartCoroutine(MoveToTarget(target.position, moveSpeed * 0.5f, false, 1.5f));
                isPatternUsing = true;
                break;
        }

        Debug.Log(attackPattern + " 패턴, 목적지 : " + destination);
    }

    IEnumerator MoveToTarget(Vector3 targetPos, float speed, bool useSmooth, float smoothTime)
    {
        isArrived = false;
        float thresholdSqr = arriveThreshold * arriveThreshold;
        Vector3 velocity = Vector3.zero;

        while ((targetPos - transform.position).sqrMagnitude > thresholdSqr)
        {
            if (useSmooth)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            }

            yield return null;
        }

        transform.position = targetPos;
        positionCur = targetPos;
        moveRoutine = null;
        isArrived = true;

        if (attackPattern == BossMoveType.Gatling || attackPattern == BossMoveType.Airborne)
        {
            patternAttackCountCur++;
        }

        if (attackPattern != BossMoveType.Normal && attackPattern != BossMoveType.Pattern)
        {
            if (isPatternUsing)
            {
                if (patternAttackCount <= patternAttackCountCur)
                {
                    ReturnToNormal();
                }
                else
                {
                    isAttackReady = ChectAttackReady();
                }

            }
        }
    }

    void ReturnToNormal()
    {
        /*
        switch (attackPattern)
        {
            case BossMoveType.Airborne:
                MoveTo(center2, BossMoveType.Pattern);
                break;
            case BossMoveType.MissileL:
                MoveTo(centerL, BossMoveType.Pattern);
                break;
            case BossMoveType.MissileR:
                MoveTo(centerR, BossMoveType.Pattern);
                break;
            case BossMoveType.Gatling:
                MoveTo(center2, BossMoveType.Pattern);
                break;
            case BossMoveType.Charge:
                MoveTo(center2, BossMoveType.Pattern);
                break;
            default:
                break;
        }
        */

        isAttackReady = false;
        isPatternUsing = false;
        isMissilePosition = false;
        normalAttackCountCur = 0;
        patternAttackCountCur = 0;
        aniCon.SetTrigger("Idle");
        patternStack++;


        Debug.Log("패턴 종료, 복귀");
    }

    bool ChectAttackReady()
    {
        bool _value = true;

        if(attackPattern == BossMoveType.MissileL || attackPattern == BossMoveType.MissileR)
        {
            _value = isMissilePosition;
        }

        return _value;
    }

    public void MissileShoot()
    {
        aniCon.SetTrigger("RocketShoot");
    }
}
