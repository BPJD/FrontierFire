using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    [SerializeField] GameObject item;
    [SerializeField] GameObject item2;
    [SerializeField] GameObject item3;
    [SerializeField] GameObject item4;
    Transform playerTr;

    private void Start()
    {
        playerTr = GameObject.FindGameObjectWithTag(Data_Strings.playerTag).transform;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Instantiate(item, playerTr.position, Quaternion.identity);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Instantiate(item2, playerTr.position, Quaternion.identity);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            Instantiate(item3, playerTr.position, Quaternion.identity);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            Instantiate(item4, playerTr.position, Quaternion.identity);
        }
    }
}
