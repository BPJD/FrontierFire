using UnityEngine;

public class Turning_Ball : MonoBehaviour
{
    Transform tr;
    [SerializeField] float turnSpeed = 5f; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        tr.Rotate(Vector3.forward * turnSpeed * Time.fixedDeltaTime);
    }
}
