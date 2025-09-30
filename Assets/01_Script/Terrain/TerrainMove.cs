using UnityEngine;

public class TerrainMove : MonoBehaviour
{
    private Rigidbody rigid;
    [SerializeField] Vector3 moveDirection = Vector3.zero;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        Vector3 moveVector = moveDirection * Time.fixedDeltaTime;
        rigid.MovePosition(transform.position + moveVector);
    }
}
