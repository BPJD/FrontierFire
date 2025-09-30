using UnityEngine;

public class Debug_WeaponLook : MonoBehaviour
{
    Transform tr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
    }

    // Update is called once per frame
    void Update()
    {
        tr.LookAt(GetComponentInParent<PlayerLookMouse>().targetPos);
    }
}
