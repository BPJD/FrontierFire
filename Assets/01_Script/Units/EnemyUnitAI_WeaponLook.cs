using UnityEngine;

public class EnemyUnitAI_WeaponLook : MonoBehaviour
{
    bool isAiming = false;
    Animator aniCon;
    Transform targetTr;
    [SerializeField] Transform armTr;

    Transform rHandPos;
    Transform lHandPos;

    Vector3 aimingRevision = new Vector3(0f, 1.5f, 0f);




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aniCon = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAiming && targetTr != null && !gameObject.CompareTag("Dead"))
        {
            armTr.LookAt(targetTr.position + aimingRevision);
        }
    }

    public void SetWeaponAimStat(bool _isAiming, Transform target)
    {
        isAiming = _isAiming;
        targetTr = target;
        if (!isAiming)
        {
            armTr.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    public void SetWeaponHandPoint(Transform right, Transform left)
    {
        rHandPos = right;
        lHandPos = left;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (rHandPos != null)
        {
            //왼손 위치 IK 적용
            aniCon.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            aniCon.SetIKPosition(AvatarIKGoal.LeftHand, lHandPos.position);

            //오른손 위치 IK 적용
            aniCon.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            aniCon.SetIKPosition(AvatarIKGoal.RightHand, rHandPos.position);

            if(targetTr != null)
            {
                //캐릭터가 타겟을 바라보도록 설정
                aniCon.SetLookAtWeight(1);
                aniCon.SetLookAtPosition(targetTr.position + aimingRevision);
            }
        }
    }
}
