using UnityEngine;
using UnityEditor;

public class DisableParticleShadows
{
    [MenuItem("Tools/Disable Particle Shadows (Selected)")]
    static void DisableShadows()
    {
        GameObject[] selected = Selection.gameObjects;

        int count = 0;

        foreach (GameObject go in selected)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                if (r is ParticleSystemRenderer)
                {
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    r.receiveShadows = false;
                    count++;
                }
            }
        }

        Debug.Log($"Particle Shadow Disabled: {count}");
    }
}