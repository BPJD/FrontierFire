using UnityEngine;

public class PlayerStageOut : MonoBehaviour
{
    Transform tr;
    public Vector3 returnPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
    }

    private void FixedUpdate()
    {
        if(tr.position.y < -20f)
        {
            returnPos.z = 0f;
            tr.position = returnPos;
        }
    }
}
