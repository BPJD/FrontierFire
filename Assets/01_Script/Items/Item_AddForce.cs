using UnityEngine;

public class Item_AddForce : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] float forceXmin = -3f;
    [SerializeField] float forceXmax = 3f;

    [SerializeField] float forceYmin = 5f;
    [SerializeField] float forceYmax = 8f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        float valueX = Random.Range(forceXmin, forceXmax);
        float valueY = Random.Range(forceYmin, forceYmax);

        rb.linearVelocity += new Vector3(valueX, valueY, 1f);
    }

}
