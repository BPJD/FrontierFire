using UnityEngine;

public class Terrain_AutoTilling : MonoBehaviour
{
    [SerializeField] float tillingScale = 1f;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material mat = renderer.material;
        Vector3 scale = transform.lossyScale;

        mat.mainTextureScale = new Vector2(scale.x, scale.y) * tillingScale;
    }
}
