using UnityEngine;

public class TerrainDownPlatform_Trigger : MonoBehaviour
{
    [SerializeField] BoxCollider platformCollider;

    string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            platformCollider.enabled = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            platformCollider.enabled = true;
        }
    }
}
