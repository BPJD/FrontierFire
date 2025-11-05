using UnityEngine;

public class BossCorePhaseEndProj : MonoBehaviour
{
    [SerializeField] ParticleSystem explodeParticle;

    private void OnTriggerEnter(Collider other)
    {

        BossCorePhaseEndRange pattern = GameObject.FindGameObjectWithTag("BossGimmick").GetComponent<BossCorePhaseEndRange>();
        pattern.PlayerThrow();

        explodeParticle.Play(true);

        Destroy(gameObject, 3f);
    }
}
