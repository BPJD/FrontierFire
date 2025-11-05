using MathNet.Numerics.Providers.SparseSolver;
using UnityEngine;

public class DroneLookUnit : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 10f;
    DroneMoveSystem moveSystem;
    TurretAttackSystem turretAttackSystem;

    [SerializeField] float maxXAngle = 5f;

    private void Start()
    {
        moveSystem = GetComponent<DroneMoveSystem>();
        turretAttackSystem = GetComponent<TurretAttackSystem>();
    }

    void Update()
    {
        if(moveSystem.target != null && !turretAttackSystem.isDead)
        {
            Vector3 directionToPlayer = moveSystem.target.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            Quaternion smoothRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            Vector3 euler = smoothRotation.eulerAngles;
            euler.x = ClampAngle(euler.x, -maxXAngle, maxXAngle);

            transform.rotation = Quaternion.Euler(euler);
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        angle = Mathf.Clamp(angle, min, max);
        return angle < 0f ? angle + 360f : angle;
    }
}
