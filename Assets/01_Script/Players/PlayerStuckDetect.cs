using UnityEngine;

public class PlayerStuckDetect : MonoBehaviour
{
    Transform playerTr;
    private void Awake()
    {
        playerTr = transform.parent;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
        {
            playerTr.position += Vector3.up;
        }
    }
}
