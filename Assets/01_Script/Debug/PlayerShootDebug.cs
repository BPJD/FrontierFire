using UnityEngine;

public class PlayerShootDebug : MonoBehaviour
{
    [SerializeField] Transform bulletTr;
    [SerializeField] GameObject bullet;
    [SerializeField] int RPM = 600;

    [SerializeField] AudioSource shootSfx;

    float rps;




    // Update is called once per frame
    void Update()
    {
        rps = RPM / 60f;

        float interval = 1f / rps;
        if (Time.time % interval < Time.deltaTime)
        {
            shootSfx.Play();
        }
    }
}
