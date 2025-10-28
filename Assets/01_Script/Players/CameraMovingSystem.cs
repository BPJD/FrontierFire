using Unity.Cinemachine;
using UnityEngine;

public class CameraMovingSystem : MonoBehaviour
{
    Transform tr;
    public PlayerLookMouse lookMouse;

    //[SerializeField] CinemachineFollow camFollow;
    [SerializeField] CinemachinePositionComposer positionComposer;

    public bool isCamRangeUp { get; private set; } = false;
    public bool isSniAiming = false;
    [SerializeField] PlayerMove playerMoveSystem;


    float _range = 5f;

    public float cameraRange
    {
        get { return _range; }
        set { _range = value; }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = GetComponent<Transform>();
        if(lookMouse == null || playerMoveSystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            lookMouse = player.GetComponent<PlayerLookMouse>();
            playerMoveSystem = player.GetComponent<PlayerMove>();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 playerPos = lookMouse.playerTr.position + Vector3.up;
        if (playerMoveSystem.isAiming)
        {
            float _camRangeRevision = 1f;
            if (isSniAiming && isCamRangeUp)
            {
                _camRangeRevision = 4f;
            }

            Vector3 direction = (lookMouse.targetPos - playerPos).normalized;
            float actualDistance = Vector3.Distance(playerPos, lookMouse.targetPos);

            // cameraRange와 actualDistance 중 더 작은 값을 사용
            float distanceToMove = Mathf.Min(cameraRange * _camRangeRevision, actualDistance);

            tr.position = playerPos + (direction * distanceToMove);

            if (lookMouse.isAimClose)
            {
                Debug.DrawLine(playerPos, tr.position, Color.red);
            }
            else
            {
                Debug.DrawLine(playerPos, tr.position, Color.green);
            }
        }

        else
        {
            tr.position = playerPos;
        }

        /*

        if (Input.GetButtonDown("Fire2"))
        {
            //    camFollow.TrackerSettings.PositionDamping = new Vector3(2f, 1f, 0f);
            positionComposer.Damping = new Vector3(2f, 1f, 0f);
            playerMoveSystem.isSprintable = false;
        }

        if (Input.GetButtonUp("Fire2"))
        {
            //    camFollow.TrackerSettings.PositionDamping = new Vector3(1f, 0.5f, 0f);
            positionComposer.Damping = new Vector3(1f, 0.5f, 0f);
            playerMoveSystem.isSprintable = true;
        }
        */
    }

    public void CamControlSet(bool _isCamRangeUp)
    {
        isCamRangeUp = _isCamRangeUp;
    }

    public void CamSpeedSet(bool _isAimKeyDown)
    {
        Vector3 speed = _isAimKeyDown ? new Vector3(2f, 1f, 0f) : new Vector3(1f, 0.5f, 0f);
        positionComposer.Damping = speed;

        playerMoveSystem.isSprintable = !_isAimKeyDown;
    }
}
