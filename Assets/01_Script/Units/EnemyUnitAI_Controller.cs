using UnityEngine;
using System.Collections;

public class EnemyUnitAI_Controller : MonoBehaviour
{
    [SerializeField] bool isDebug = false;

    EnemyUnitMove unitMove;
    EnemyAttackSystem atkStat;
    UnitStatus unitStat;
    EnemyUnitAI_WeaponLook aiWeaponAim;

    [SerializeField] Transform eyesOfUnit;
    Transform thisTr;
    Transform target;

    //public bool isMovable = false;
    public bool isJumpable = false;
    bool isFocusing = false;
    public bool isEngage { get; private set; } = false;
    public bool isNotMove = false;

    float senseDistance = 10f; // 감지 거리
    float unitAttackRange = 3f;
    [SerializeField] LayerMask hitLayers; // 감지할 레이어 지정

    int chaseStack = 100;
    int chaseStackCur = 0;
    int returnStack = 100;
    int returnStackCur = 0;

    float moveAniSpd = 0.5f;

    bool isChaseByAggro = false; // 자신이 감지를 시작한 유닛인지 여부
    private EnemyAIBroadcastManager broadcastManager;


    [SerializeField] Transform holdPosition;
    Vector3 holdPositionVector;

    WaitForSeconds focusDelay = new WaitForSeconds(0.25f);
    WaitForSeconds aiActionDelay = new WaitForSeconds(0.1f);

    public enum UnitState { Idle, Patrol, Chase, Attack, Return, Dead }
    UnitState stateMain = UnitState.Idle;
    public UnitState state = UnitState.Idle;
    UnitState stateCur = UnitState.Idle;

    public UnitState StateChange
    {
        get => state;
        set
        {
            // 죽었으면 더 이상 상태 못 바꿈
            if (state == UnitState.Dead) return;

            // Dead로 바뀌는 순간 모든 행동 중지
            if (value == UnitState.Dead)
            {
                StopAllCoroutines(); // 혹은 모든 상태 루틴 중단
            }

            state = value;
        }
    }


    private void Awake()
    {

        unitMove = GetComponent<EnemyUnitMove>();
        thisTr = transform;
        broadcastManager = GetComponentInParent<EnemyAIBroadcastManager>();
        aiWeaponAim = GetComponent<EnemyUnitAI_WeaponLook>();
        unitStat = GetComponent<UnitStatus>();

        if (holdPosition == null)
            holdPositionVector = thisTr.position;
        else
            holdPositionVector = holdPosition.position;

    }

    private void Start()
    {
        StartCoroutine(AI_Action());
        atkStat = GetComponent<EnemyAttackSystem>();
        unitAttackRange = atkStat.w_range;
        stateMain = state;
        senseDistance = atkStat.sightRange;
    }

    public void PlayerApproach(bool isApproaching)
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform;

        isFocusing = isApproaching;
        if (isApproaching)
            StartCoroutine(PlayerFocusing());
    }

    IEnumerator AI_Action()
    {
        float _randDelay = Random.Range(0f, 1f);
        yield return new WaitForSeconds(_randDelay);
        while (true)
        {
            if (stateCur != UnitState.Dead)
            {
                if (state != stateCur)
                {
                    chaseStackCur = 0;
                    returnStackCur = 0;
                    stateCur = state;

                    switch (stateCur)
                    {
                        case UnitState.Idle:
                            moveAniSpd = 0.5f;
                            unitMove.MoveStatusSet(true, moveAniSpd);
                            break;

                        case UnitState.Patrol:
                            moveAniSpd = 0.75f;
                            unitMove.MoveStatusSet(false, moveAniSpd, true);
                            break;

                        case UnitState.Chase:
                            if (isNotMove)
                            {
                                moveAniSpd = 0.5f;
                            }
                            else
                            {
                                moveAniSpd = 1f;
                            }
                            isEngage = false;
                            unitMove.MoveStatusSet(false, moveAniSpd);
                            StartCoroutine(AI_Chase());
                            break;

                        case UnitState.Attack:
                            moveAniSpd = 0.5f;
                            StartCoroutine(AI_Attack());
                            unitMove.MoveStatusSet(true, moveAniSpd);
                            isEngage = true;
                            break;

                        case UnitState.Return:
                            moveAniSpd = 0.75f;
                            isEngage = false;
                            unitMove.MoveStatusSet(false, moveAniSpd);
                            StartCoroutine(AI_Return());
                            break;

                        case UnitState.Dead:
                            moveAniSpd = 0.5f;
                            unitMove.MoveStatusSet(true, moveAniSpd);
                            isFocusing = false;
                            isEngage = false;
                            aiWeaponAim.SetWeaponAimStat(false, null);
                            atkStat.SetWeaponPropToHand();
                            StopAllCoroutines();
                            break;
                    }
                }

                if (unitStat != null && unitStat.isUnitHit && !isEngage)
                {
                    unitStat.isUnitHit = false;

                    if (!isChaseByAggro)
                    {
                        isChaseByAggro = true; // 처음 감지한 유닛만
                        broadcastManager.BroadcastEngage(thisTr.position);
                    }
                    if (target == null)
                    {
                        target = GameObject.FindGameObjectWithTag("Player").transform;
                    }
                    StateChange = UnitState.Chase;
                }
            }
            


            yield return aiActionDelay;
        }
    }

    IEnumerator PlayerFocusing() // 플레이어가 근처에 들어오면 감지 시작
    {
        while (isFocusing && stateCur != UnitState.Dead)
        {
            //RotateTowards(target.position);

            if (CheckPlayerVisible() && stateCur != UnitState.Attack)
            {

                if (!isChaseByAggro)
                {
                    isChaseByAggro = true; // 처음 감지한 유닛만

                    if (!isDebug)
                    {
                        broadcastManager.BroadcastEngage(thisTr.position);
                    }
                    
                }
                StateChange = UnitState.Chase;
                //chaseStackCur = 0;
            }

            yield return focusDelay;
        }
    }

    IEnumerator AI_Chase() // 추격 중, 추적 실패 시 Return 상태로 전환
    {
        while (stateCur == UnitState.Chase)
        {
            if(target != null)
            {
                RotateTowards(target.position);
                aiWeaponAim.SetWeaponAimStat(true, target);

                if (CheckPlayerInRange())
                {
                    StateChange = UnitState.Attack;
                }
                else
                {
                    chaseStackCur += 3;
                    if (chaseStackCur >= chaseStack)
                    {
                        StateChange = UnitState.Return;
                    }
                }

                yield return focusDelay;
            }
        }
    }

    IEnumerator AI_Return() // 원래 위치로 복귀
    {
        while (stateCur == UnitState.Return)
        {
            RotateTowards(holdPositionVector);
            aiWeaponAim.SetWeaponAimStat(false, null);

            float sqrDistance = (holdPositionVector - thisTr.position).sqrMagnitude;
            float checkDistanceSqr = 3f * 3f;

            if (sqrDistance <= checkDistanceSqr)
            {
                StateChange = stateMain;
                isChaseByAggro = false;
            }
            else
            {
                returnStackCur += 5;
                if (returnStackCur > returnStack)
                {
                    thisTr.position = holdPositionVector;
                }
            }

            if (CheckPlayerVisible() && !isEngage)
            {
                StateChange = UnitState.Chase;
                //chaseStackCur = 0;
            }

            yield return focusDelay;
        }
    }

    IEnumerator AI_Attack()
    {
        while (stateCur == UnitState.Attack)
        {
            RotateTowards(target.position);
            aiWeaponAim.SetWeaponAimStat(true, target);

            if (CheckPlayerInRange())
            {
                if (!isEngage)
                {
                    atkStat.UnitCombat(true, target);
                    isEngage = true;
                    unitMove.MoveStatusSet(true, moveAniSpd);
                }
            }
            else
            {
                atkStat.UnitCombat(false, target);
                //unitMove.MoveStatusSet(false, moveAniSpd);
                StateChange = UnitState.Chase;
                //isEngage = false;
            }

            yield return focusDelay;
        }
    }

    void RotateTowards(Vector3 targetPos) // 대상 방향으로 Y축 기준 회전
    {
        Vector3 dir = targetPos + Vector3.up - thisTr.position;
        if (dir.x > 0)
            thisTr.rotation = Quaternion.LookRotation(Vector3.right);
        else
            thisTr.rotation = Quaternion.LookRotation(Vector3.left);


        if(stateCur != UnitState.Return)
        {
            //unitMove.MoveStatusSet(Mathf.Abs(dir.sqrMagnitude) < 2.25f, moveAniSpd);
        }

    }

    bool CheckPlayerVisible()
    {
        if (target == null) return false;

        Vector3 eyeToTarget = (target.position + Vector3.up * 1.3f) - eyesOfUnit.position;
        Vector3 direction = eyeToTarget.normalized;

        float radius = 0.3f;

        if (Physics.SphereCast(eyesOfUnit.position, radius, direction, out RaycastHit hit, senseDistance, hitLayers))
        {
            Debug.DrawLine(eyesOfUnit.position, hit.point, Color.yellow, 1.0f);
            return hit.transform == target;
        }

        return false;
    }


    bool CheckPlayerInRange()
    {
        if (target == null) return false;

        Vector3 eyeToTarget = (target.position + Vector3.up * 1.3f) - eyesOfUnit.position;
        Vector3 direction = eyeToTarget.normalized;

        float sqrDistance = (target.position - transform.position).sqrMagnitude;
        float minDistanceSqr = 1f * 1f; // 보완용 최소 거리 (조절 가능)

        // 1. Ray 감지 우선
        if (Physics.Raycast(eyesOfUnit.position, direction, out RaycastHit hit, unitAttackRange, hitLayers))
        {
            Debug.DrawLine(eyesOfUnit.position, hit.point, Color.blue, 1.0f);
            if (hit.transform == target)
                return true;
        }

        // 2. 너무 가까우면 강제 감지 성공 처리
        if (sqrDistance < minDistanceSqr)
        {
            Debug.DrawLine(eyesOfUnit.position, target.position, Color.green, 1.0f);
            return true;
        }

        return false;
    }

    public void ForceChase()
    {
        if (stateCur == UnitState.Dead) return;

        // 이미 Chase 중이거나 Attack 중이면 중복 호출 방지
        if (state == UnitState.Chase || state == UnitState.Attack || isEngage)
            return;

        isChaseByAggro = true; //이걸로 유입된 놈들은 어그로 전파 못함

        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        state = UnitState.Chase;

        /**
        // **여기서 Broadcast 호출 전에 자신이 Broadcast한 유닛인지 체크**
        if (!isBroadcastSource)
        {
            isBroadcastSource = true;
            if (broadcastManager != null)
                broadcastManager.BroadcastEngage(transform.position);

            isBroadcastSource = false;
        }

        **/
    }


    public bool IsDead()
    {
        return stateCur == UnitState.Dead;
    }
    public bool IsEngagingOrChasing()
    {
        return state == UnitState.Chase || state == UnitState.Attack || isEngage;
    }

}
