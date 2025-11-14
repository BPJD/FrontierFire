using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    Transform playerTr;
    Transform tr;

    [SerializeField] Vector3 followVector = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTr = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).transform;
        tr = transform;

        tr.parent = playerTr;
        tr.localPosition = followVector;
        
    }
}