using UnityEngine;

public class BossCorePhaseEndProj : MonoBehaviour
{
    [SerializeField] ParticleSystem explodeParticle;

    private void OnTriggerEnter(Collider other)
    {

        BossCorePhaseEndRange pattern = GameObject.FindGameObjectWithTag("BossGimmick").GetComponent<BossCorePhaseEndRange>();
        pattern.PlayerThrow();

        explodeParticle.Play(true);

        Debug.Log(pattern.gameObject.name);

        Destroy(gameObject, 3f);
    }
}
