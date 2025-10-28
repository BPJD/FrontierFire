using UnityEngine;
using System.Collections;

public class Terrain_AutoTilling : MonoBehaviour
{
    [SerializeField] float tillingScale = 1f;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material mat = renderer.material;
        Vector3 scale = transform.lossyScale;

        mat.mainTextureScale = new Vector2(scale.x, scale.y) * tillingScale;

        StartCoroutine(Debug_Tilling());
    }

    IEnumerator Debug_Tilling()
    {
        while (true)
        {

            Renderer renderer = GetComponent<Renderer>();
            Material mat = renderer.material;
            Vector3 scale = transform.lossyScale;

            mat.mainTextureScale = new Vector2(scale.x, scale.y) * tillingScale;

            yield return new WaitForSeconds(0.5f);
        }
    }
}
