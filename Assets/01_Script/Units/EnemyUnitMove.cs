using UnityEngine;

public class EnemyUnitMove : MonoBehaviour
{
    Transform tr;
    UnitStatus unitStat;
    EnemyUnitAI_Controller controller;
    Animator unitAniCon;
    float moveSpeed = 5f;
    float speedCur = 1f;
    bool isNotMove = false;
    public bool isMovable { get; set; } = false;
    public bool isJumpable { get; set; } = false;
    public bool isIdle = true;

    public Vector3 moveDirection { get; set; } = Vector3.forward;
    public bool isMoveForward { get; set; } = true;



    private void Awake()
    {
        unitAniCon = GetComponent<Animator>();
        controller = GetComponent<EnemyUnitAI_Controller>();
        unitStat = GetComponent<UnitStatus>();
    }

    private void Start()
    {
        tr = transform;
        //isMovable = controller.isMovable;
        isJumpable = controller.isJumpable;
        moveSpeed = unitStat.moveSpeed;
        isNotMove = controller.isNotMove;

    }

    private void Update()
    {
        if (/*isMovable && */!isIdle && !isNotMove)
        {
            tr.Translate(moveDirection * moveSpeed * speedCur * Time.deltaTime);
        }

        
        
    }

    public void MoveStatusSet(bool _isStopped, float _moveSpeed, bool isPatrol = false)
    {
        if (!isNotMove)
        {
            isIdle = _isStopped;

            if (_moveSpeed >= 0.9f)
            {
                speedCur = 1.5f;
            }
            else if (isPatrol)
            {
                speedCur = 0.5f;
            }
            else
            {
                speedCur = 1f;
            }

            unitAniCon.SetFloat("Move", _moveSpeed);
        }
        
    }

    //public void RunForChasePlayer(bool _isChasing)
    //{
    //    isIdle = false;
    //    switch (_isChasing)
    //    {
    //        case true:
    //            speedCur = 1.5f;
    //            unitAniCon.SetFloat("Move", 1f);
    //            break;
    //        case false:
    //            speedCur = 1f;
    //            unitAniCon.SetFloat("Move", 0.75f);
    //            break;
    //    }
    //}

    //public void StopMove(bool _isStop)
    //{
    //    isIdle = _isStop;
    //    switch (_isStop)
    //    {
    //        case true:
    //            unitAniCon.SetFloat("Move", 0.5f);
    //            break;
    //        case false:
    //            unitAniCon.SetFloat("Move", 0.75f);
    //            break;
    //    }
    //}


}
