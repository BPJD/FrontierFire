using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }
}