using UnityEngine;

public class TrailorCamera : MonoBehaviour
{
    Transform tr;
    public Transform target;
    public bool isFollowing = false;
    public Vector3 offset = Vector3.zero;

    private void Start()
    {
        tr = GetComponent<Transform>();
    }
    // Update is called once per frame
    void Update()
    {
        if(target != null)
        {
            isFollowing = true;
            tr.position = target.position + offset;
        }
        else
        {
            isFollowing = false;

        }

    }
}
