using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Video;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PaintScript : MonoBehaviour
{
    private RawImage canvas;
    public int brushSize = 10;
    public Color paintColor = Color.blue;
    public int textureWidth = 512;
    public int textureHeight = 512;
    private Texture2D texture;

    void Start()
    {
        canvas = GetComponent<RawImage>();
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color[] fillColor = new Color[textureWidth  * textureHeight];   
        for (int i = 0; i < textureWidth; i++)
        {
            fillColor[i] = Color.white; //fill canvas white at start
        }
        texture.SetPixels(fillColor);
        texture.Apply();
        canvas.texture = texture;
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.rectTransform, Input.mousePosition, null, out mousePos);
            Rect rect = canvas.rectTransform.rect;
            float px = (mousePos.x - rect.x) / rect.width * textureWidth;
            float py = (mousePos.y - rect.y) / rect.height * textureHeight;
            DrawCircle((int)px, (int)py);
            texture.Apply();
        }
    }
    void DrawCircle(int cx, int cy)
    {
        for (int x = -brushSize; x < brushSize; x++)
        {
            for (int y = -brushSize; y < brushSize; y++)
            {
                if (x * x + y * y <= brushSize * brushSize)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && py < textureWidth && py >= 0 && py < textureHeight)
                    {
                        texture.SetPixel(px, py, paintColor);
                    }
                }
            }
        }
    }
}
