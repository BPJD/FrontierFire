using UnityEngine;

public class Bullet_MuzzlePlayer : MonoBehaviour
{
    Bullet bullet;
    AudioSource audioPlayer;


    public void PlayMuzzleSound(AudioClip clip)
    {
        if(clip != null)
        {
            if (audioPlayer == null)
            {
                audioPlayer = gameObject.AddComponent<AudioSource>();
            }
            audioPlayer.PlayOneShot(clip);
        }
    }


    private void OnParticleSystemStopped()
    {
        if(bullet == null)
        {
            bullet = GetComponentInParent<Bullet>();
        }
        bullet.MuzzleEftisEnd();
    }
}
