using UnityEngine;

public class Item_Move : MonoBehaviour
{
    [SerializeField] float pushStrength = 1f;
    [SerializeField] float maxPushDistance = 1.5f;
    [SerializeField] Transform parentTr;

    void OnTriggerStay(Collider other)
    {
        // 여기서는 other는 무조건 Item이므로
        Vector3 directionDefault = transform.position - other.transform.position;
        Vector3 direction = new Vector3(directionDefault.x, directionDefault.y, 0f);
        float distance = direction.magnitude;

        if (distance > 0.01f)
        {
            direction.Normalize();
            parentTr.Translate(direction * pushStrength * Time.deltaTime, Space.World);
        }
    }

}
