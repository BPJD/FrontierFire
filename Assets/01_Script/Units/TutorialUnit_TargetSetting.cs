using UnityEngine;

public class TutorialUnit_TargetSetting : MonoBehaviour
{
    public enum MovingType { None, Vertical, Horizontal }

    [SerializeField] MovingType moveType;

    [SerializeField] float moveRange = 3f;
    float moveRangeSqr = 0f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] bool isHidden = false;
    [SerializeField] GameObject hitEffect;
    Transform tr;
    Vector3 defaultPos;
    Vector3 moveDir;
    bool isMoving = false;

    float turnCool = 0.2f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
        defaultPos = tr.position;
        moveRangeSqr = moveRange * moveRange;

        switch (moveType)
        {
            case MovingType.Vertical:
                moveDir = Vector3.up;
                isMoving = true;
                break;
            case MovingType.Horizontal:
                moveDir = Vector3.right;
                isMoving = true;
                break;
            default:
                moveDir = Vector3.zero;
                break;

        }

        if (isHidden)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            if(Vector3.SqrMagnitude(tr.position - defaultPos) >= moveRangeSqr && turnCool < 0f)
            {
                MoveDirTurn();
                turnCool = 0.2f;
            }
            tr.Translate(moveDir * moveSpeed * Time.deltaTime);

            turnCool -= Time.deltaTime;
        }

    }

    void MoveDirTurn()
    {
        if(moveSpeed > 0f)
        {
            moveSpeed = -moveSpeed;
        }
        else
        {
            moveSpeed = Mathf.Abs(moveSpeed);
        }
    }

    void NeutralUnitDead()
    {
        Instantiate(hitEffect, tr.position, Quaternion.identity);
        GetComponentInParent<Stage_TutorialTargetSpawn>().TargetHit(gameObject);
        Destroy(gameObject);
    }
}
