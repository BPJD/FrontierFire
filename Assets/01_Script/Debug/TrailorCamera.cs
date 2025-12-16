using UnityEngine;

public class TrailorCamera : MonoBehaviour
{
    Transform tr;
    public Transform target;
    public bool isFollowing = false;

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
            tr.position = target.position;
        }
        else
        {
            isFollowing = false;

        }

    }
}
