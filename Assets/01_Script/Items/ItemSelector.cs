using UnityEngine;

public class ItemSelector : MonoBehaviour
{
    [SerializeField] GameObject[] dropItemsList;

    [SerializeField] GameObject[] dropItems;

    [SerializeField] int itemCount;

    Transform tr;

    float explosionForce = 8f;

    private void Start()
    {
        tr = transform;
        ItemDrop();
    }

    void ItemDrop()
    {
        dropItems = new GameObject[itemCount];

        if(itemCount == 1)
        {
            int dropItem = Random.Range(0, dropItemsList.Length);
            dropItems[0] = Instantiate(dropItemsList[dropItem], tr.position, Quaternion.identity, tr);

            float angle = Random.Range(-45f, 45f);
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up; // X-Y 평면 기준

            Rigidbody rb = dropItems[0].GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction.normalized * explosionForce, ForceMode.Impulse);
            }
        }
        else
        {
            for (int i = 0; i < itemCount; i++)
            {
                int dropItem = Random.Range(0, dropItemsList.Length);

                dropItems[i] = Instantiate(dropItemsList[dropItem], tr.position, Quaternion.identity, tr);


                float angle = -45f + i * (90f / (itemCount - 1));
                Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up; // X-Y 평면 기준

                Rigidbody rb = dropItems[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(direction.normalized * explosionForce, ForceMode.Impulse);
                }
            }
        }
        
    }

    public void ItemSelected()
    {
        Destroy(gameObject);
    }
}
