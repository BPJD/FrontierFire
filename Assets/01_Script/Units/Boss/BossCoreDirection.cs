using UnityEngine;
using System.Collections;

public class BossCoreDirection : MonoBehaviour
{
    [SerializeField] ParticleSystem shieldParticle;
    [SerializeField] ParticleSystem crystalParticle;

    [SerializeField] GameObject directionUnit;
    [SerializeField] GameObject bossUnit;
    [SerializeField] Vector3 camOffset = Vector3.down;


    private void Start()
    {
        Direction_BossStage bossStage = GameObject.FindGameObjectWithTag("GameController").GetComponent<Direction_BossStage>();
        bossStage.BossStageEntry(directionUnit, bossUnit, camOffset);

        StartCoroutine(Direction());

    }

    
    IEnumerator Direction()
    {
        yield return new WaitForSeconds(3f);
        shieldParticle.Play(true);
        crystalParticle.Play(true);


    }
}
