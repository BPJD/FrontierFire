using UnityEngine;

public class BossBomberWeaponRotate : MonoBehaviour
{
    Transform target;
    Transform tr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        tr.LookAt(target.position + Vector3.up);
    }
}
