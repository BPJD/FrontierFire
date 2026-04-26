using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    [SerializeField] Color tintColor = Color.white;

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }


    Texture2D Resize(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D newTex = new Texture2D(width, height);
        newTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        newTex.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return newTex;
    }

    Texture2D TintCursor(Texture2D source, Color tint)
    {
        Texture2D newTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

        Color[] pixels = source.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] *= tint; // »ö»ó °ö (Tint)
        }

        newTex.SetPixels(pixels);
        newTex.Apply();

        return newTex;
    }
}