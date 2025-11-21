using UnityEngine;
using UnityEngine.EventSystems;

public class BossGiantMove : MonoBehaviour
{
    UnitStatus unitStat;

    [SerializeField] float moveSpeed = 5f;
    Transform tr;
    Transform playerTr;
    Animator aniCon;
    BossGiantAttackControl bossAttackAI;

    [SerializeField] float distanceToPlayer = 5f;
    float distanceCur;
    public bool isClose { get; private set; } = false;

    string ani_direction = "Direction";

    public bool isMove { get; set; } = false;

    private void Start()
    {
        tr = transform;
        unitStat = GetComponent<UnitStatus>();
        moveSpeed = unitStat.moveSpeed;
        playerTr = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).transform;
        distanceToPlayer *= distanceToPlayer;
        aniCon = GetComponent<Animator>();
        bossAttackAI = GetComponent<BossGiantAttackControl>();

        // Animator에서 behaviour 가져와서 콜백 연결
        var behaviours = aniCon.GetBehaviours<BossGiant_Anicon_Move>();
        foreach (var b in behaviours)
        {
            b.OnStart = OnStart;   // ★ 여기서 내 함수 연결
        }
    }

    private void FixedUpdate()
    {
        if (isMove)
        {
            if (!isClose)
            {
                bool _isLeft = playerTr.position.x > tr.position.x;


                Vector3 moveVector = _isLeft ? Vector3.left : Vector3.right;
                tr.Translate(moveVector * moveSpeed * Time.fixedDeltaTime);

                float _aniDirection = _isLeft ? 0f : 1f;
                aniCon.SetFloat(ani_direction, _aniDirection);
            }
            else
            {
                aniCon.SetFloat(ani_direction, 0.5f);
            }

            distanceCur = (tr.position - playerTr.position).sqrMagnitude;
            isClose = distanceToPlayer > distanceCur;

        }
    }

    public void OnStart()
    {
        isMove = true;
    }


}
