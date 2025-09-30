using UnityEditor;
using UnityEngine;
using System.Collections;

public class PlayerAnimatorLook : MonoBehaviour
{
    Animator animator;
    PlayerLookMouse lookMouse;
    [SerializeField] Transform lookPos;
    [SerializeField] Transform lookPosParent;
    Transform rHandPos;
    Transform lHandPos;

    public Transform gunRotatePivot;
    

    [SerializeField] Transform rHand;
    [SerializeField] Transform gunPar;
    [SerializeField] Transform gunPos;
    [SerializeField] Transform gunShoulder;

    bool isWeaponReady = false;

    public bool d_isLooking = false;

    Transform headBone; // 머리 본 트랜스폼 (Animator에서 가져오기 가능)
    [SerializeField] float maxLookAngle = 60f; // 좌우 최대 회전 허용각
    [SerializeField] float minVerticalAngle = -20f; // 위아래 최소 허용각
    [SerializeField] float maxVerticalAngle = 40f;  // 위아래 최대 허용각

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        lookMouse = GetComponentInParent<PlayerLookMouse>();
    }

    private void Update()
    {
        lookPosParent.LookAt(lookMouse.targetPos);
        if (isWeaponReady)
        {
            gunRotatePivot.LookAt(new Vector3(lookMouse.targetPos.x, lookMouse.targetPos.y, gunRotatePivot.position.z));
            if(gunRotatePivot.rotation.x > 0)
            {
                gunRotatePivot.localPosition = Vector3.down * gunRotatePivot.rotation.x * 0.5f;
            }
        }

        
        //rpos.LookAt(lookMouse.targetPos);

    }

    // IK 처리
    void OnAnimatorIK(int layerIndex)
    {
        if (lookMouse.targetPos != null)
        {
            if (isWeaponReady)
            {
                // 손 IK 처리
                if (lHandPos != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, lHandPos.position);
                }
                if (rHandPos != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rHandPos.position);
                }
            }

            // 머리 회전 제한 처리
            Vector3 headForward = headBone.forward;
            Vector3 dirToTarget = (lookPos.position - headBone.position).normalized;

            // 로컬 좌표계로 변환
            Vector3 localDir = headBone.InverseTransformDirection(dirToTarget);
            Vector3 clampedDir = localDir;

            // 좌우 회전 제한
            float horizontalAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
            horizontalAngle = Mathf.Clamp(horizontalAngle, -maxLookAngle, maxLookAngle);

            // 상하 회전 제한
            float verticalAngle = Mathf.Asin(localDir.y) * Mathf.Rad2Deg;
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

            // 제한된 방향 다시 월드 좌표로 변환
            Quaternion rot = Quaternion.Euler(-verticalAngle, horizontalAngle, 0f);
            clampedDir = headBone.TransformDirection(rot * Vector3.forward);

            clampedDir.z = 0f;
            clampedDir.Normalize();

            // IK 적용
            animator.SetLookAtWeight(1);
            animator.SetLookAtPosition(headBone.position + clampedDir * 10f);
        }
    }

    public void IKPositionSet(Transform LHandIK, Transform RHandIK)
    {
        rHandPos = RHandIK;
        lHandPos = LHandIK;
    }

    public void GunPositionSet(Transform _shoulder, Transform _gunPos, Transform _gunPar)
    {
        gunShoulder = _shoulder;
        gunPos = _gunPos;
        gunPar = _gunPar;
    }

    public void GunReload(bool _isActivated)
    {
        isWeaponReady = !_isActivated;
        if (_isActivated)
        {
            gunPar.parent = rHand;
            gunRotatePivot.localPosition = Vector3.zero;
        }
        else
        {
            gunPar.parent = gunShoulder;
            gunPar.localPosition = gunPos.localPosition;
            gunPar.localRotation = gunPos.localRotation;
            
        }

    }

    public void GunAnimationReady(bool _isReady)
    {
        isWeaponReady = _isReady;
        if (!isWeaponReady)
        {
            // IK 해제 → 원래 애니메이션 상태로 복귀
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        }
    }
    

}

