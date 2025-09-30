using MathNet.Numerics.Statistics;
using UnityEngine;

public class Boss_DeathParticlePlayer : MonoBehaviour
{
    [SerializeField] GameObject bossMesh;

    private void OnParticleSystemStopped()
    {
        if (bossMesh == null)
        {
            bossMesh = gameObject.transform.parent.gameObject;
        }

        gameObject.transform.parent.SetParent(null);
        bossMesh.SetActive(false);
    }
}
