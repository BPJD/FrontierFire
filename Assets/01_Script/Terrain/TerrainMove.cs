using UnityEngine;
using System.Collections;

public class TerrainMove : MonoBehaviour
{
    Transform tr;

    [SerializeField] float speed = 3f;
    [SerializeField] float delay = 0f;
    float delayCur = 0f;
    bool isMoving = false;
    bool isReturning = false;
    
    private Rigidbody rigid;
    [SerializeField] Vector3 moveTo = Vector3.zero;

    Vector3 moveDirection;
    Vector3 startPos;
    Vector3 endPos;

    Vector3 destinationCur;

    float reqDistance;

    private void Awake()
    {
        if(moveTo == Vector3.zero)
        {
            this.enabled = false;
            return;
        }

        tr = transform;

        delayCur = delay;

        startPos = tr.localPosition;
        endPos = tr.localPosition + moveTo;

        rigid = GetComponent<Rigidbody>();

        reqDistance = (startPos - endPos).sqrMagnitude;

        SetDestination();
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            Vector3 moveVector = moveDirection * speed * Time.fixedDeltaTime;
            rigid.MovePosition(tr.position + moveVector);
            float _distanceCur = (tr.localPosition - destinationCur).sqrMagnitude;

            if (reqDistance < _distanceCur)
            {
                isMoving = false;
                isReturning = !isReturning;
                delayCur = delay;
                SetDestination();
                tr.localPosition = destinationCur;
            }
        }
        else
        {
            delayCur -= Time.fixedDeltaTime;
            if (delayCur < 0)
            {
                isMoving = true;
                Debug.DrawLine(startPos, startPos + moveTo, Color.red, delay);
            }
        }
    }

    void SetDestination()
    {
        destinationCur = isReturning ? endPos : startPos;
        moveDirection = isReturning ? -moveTo.normalized : moveTo.normalized;
    }
}
