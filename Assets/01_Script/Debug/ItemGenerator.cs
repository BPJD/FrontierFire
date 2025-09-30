using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    [SerializeField] GameObject item;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Instantiate(item, new Vector3(0, 2, 0), Quaternion.identity);
        }
    }
}
