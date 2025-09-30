using UnityEngine;

public class Bullet_MuzzlePlayer : MonoBehaviour
{
    Bullet bullet;

    private void OnParticleSystemStopped()
    {
        if(bullet == null)
        {
            bullet = GetComponentInParent<Bullet>();
        }
        bullet.MuzzleEftisEnd();
    }
}
