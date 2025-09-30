using UnityEngine;

public class ItemCollideController : MonoBehaviour
{
    [SerializeField] GameObject itemCollider;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            itemCollider.SetActive(true);
        }
    }
}
