using UnityEngine;

public class ParticleTrailBuildFix : MonoBehaviour
{
    [SerializeField] private ParticleSystem targetParticle;

    [Header("Build Correction")]
    [SerializeField] private float buildSimulationSpeed = 0.85f;
    [SerializeField] private float buildTrailLifetimeMultiplier = 0.65f;

    private void Awake()
    {
        if (targetParticle == null)
            targetParticle = GetComponent<ParticleSystem>();

        if (targetParticle == null)
            return;

#if !UNITY_EDITOR
        ApplyBuildCorrection();
#endif
    }

    private void ApplyBuildCorrection()
    {
        var main = targetParticle.main;
        main.simulationSpeed *= buildSimulationSpeed;

        var trails = targetParticle.trails;
        if (trails.enabled)
        {
            var lifetime = trails.lifetime;

            if (lifetime.mode == ParticleSystemCurveMode.Constant)
            {
                lifetime.constant *= buildTrailLifetimeMultiplier;
            }
            else if (lifetime.mode == ParticleSystemCurveMode.TwoConstants)
            {
                lifetime.constantMin *= buildTrailLifetimeMultiplier;
                lifetime.constantMax *= buildTrailLifetimeMultiplier;
            }

            trails.lifetime = lifetime;
        }
    }
}