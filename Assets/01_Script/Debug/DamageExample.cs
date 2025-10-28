using DamageNumbersPro;
using UnityEngine;

public class DamageExample : MonoBehaviour
{

    //Assign prefab in inspector.
    public DamageNumber numberPrefab;

    void Update()
    {
        //On leftclick.
        if (Input.GetMouseButtonDown(0))
        {
            //Spawn new popup at transform.position with a random number between 0 and 100.
            DamageNumber damageNumber = numberPrefab.Spawn(transform.position, Random.Range(1, 100));
        }
    }
}
