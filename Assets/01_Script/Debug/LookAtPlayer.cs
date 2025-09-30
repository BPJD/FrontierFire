using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{

    [SerializeField] Transform target;
    Transform tr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            tr.LookAt(target.position + Vector3.up);
        }
    }
}
