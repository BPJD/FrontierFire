using UnityEngine;

public class Data_DropAmmo : MonoBehaviour
{
    public float GameDropRate { get; set; } = 1f;
    [SerializeField] GameObject[] dropAmmoProps;

    public GameObject GetAmmoType(float weight)
    {
        float randValue = Random.Range(0f, 1f);
        if(randValue <= weight)
        {
            return dropAmmoProps[1];
        }
        else
        {
            return dropAmmoProps[0];
        }
        

    }
}
