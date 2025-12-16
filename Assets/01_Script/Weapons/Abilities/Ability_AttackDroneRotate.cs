using UnityEngine;

public class Ability_AttackDroneRotate : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 10f;

    [SerializeField] float maxXAngle = 5f;

    bool isPlayerDead = false;

    Transform tr;

    PlayerLookMouse playerLook;

    private void Start()
    {
        tr = transform;
        playerLook = GetComponentInParent<PlayerLookMouse>();
    }

    void Update()
    {
        if (!isPlayerDead)
        {
            Vector3 directionToAim = playerLook.targetPos - tr.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToAim);

            Quaternion smoothRotation = Quaternion.Slerp(tr.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            Vector3 euler = smoothRotation.eulerAngles;
            euler.x = ClampAngle(euler.x, -maxXAngle, maxXAngle);

            tr.rotation = Quaternion.Euler(euler);
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        angle = Mathf.Clamp(angle, min, max);
        return angle < 0f ? angle + 360f : angle;
    }

    public void DroneStop()
    {
        isPlayerDead = true;
    }
}
