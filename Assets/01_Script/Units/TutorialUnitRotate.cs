using UnityEngine;

public class TutorialUnitRotate : MonoBehaviour
{
    Transform tr;
    [SerializeField] float rotSpeed = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = transform;
    }

    // Update is called once per frame
    void Update()
    {
        tr.Rotate(Vector3.up * rotSpeed * Time.deltaTime);
    }
}
