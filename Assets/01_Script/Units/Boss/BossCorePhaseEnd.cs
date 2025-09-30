using Kamgam.HitMe;
using UnityEngine;
using System.Collections;

public class BossCorePhaseEnd : MonoBehaviour
{
    AnimationProjectileSource proj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        proj = GetComponent<AnimationProjectileSource>();
    }

    public void EndPhaseShot()
    {
        proj.Spawn();
    }

    IEnumerator Debug_Proj()
    {
        while (true)
        {
            proj.Spawn();
            yield return new WaitForSeconds(3f);
        }



    }
}
