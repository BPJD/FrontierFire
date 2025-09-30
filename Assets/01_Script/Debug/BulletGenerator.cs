using UnityEngine;
using System.Collections;

public class BulletGenerator : MonoBehaviour
{
    public GameObject bullet;
    




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(BulletShot());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator BulletShot()
    {
        while (true)
        {
            Instantiate(bullet, transform.position, transform.rotation);
            yield return new WaitForSeconds(1f);
        }
    }

    

    

}
