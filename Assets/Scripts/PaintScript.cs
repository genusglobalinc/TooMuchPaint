using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Video;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PaintScript : MonoBehaviour
{
    private RawImage canvas;
    public int brushSize = 10;
    
    public int textureWidth = 512;
    public int textureHeight = 512;
    private Texture2D texture;

    private PlayerController playerController;
    private UIManager uiManager;
    public Color paintColor = Color.red;
    private bool isPainting = false;
    private bool isErasing = false;
    private Color previousColor;

    public Slider brushSlider;
    void Start()
    {
        canvas = GetComponent<RawImage>();
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        ClearCanvas(); // Use our clear method to initialize
        canvas.texture = texture;
        
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // moved these from playercontroller
        {
            StartPainting();
        }
        else if (Input.GetMouseButton(0))
        {
            ContinuePainting();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopPainting();
        }
        if (Input.GetMouseButtonDown(1))
        {
            StartErasing();
            previousColor = paintColor;
            paintColor = Color.white;
        }
        else if (Input.GetMouseButton(1))
        {
            ContinueErasing();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            StopErasing();
            paintColor = previousColor;
            
        }
        if (isPainting)
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.rectTransform, Input.mousePosition, Camera.main, out mousePos);
            Rect rect = canvas.rectTransform.rect;
            float px = ((mousePos.x + rect.width/2) / rect.width) * textureWidth;
            float py = ((mousePos.y + rect.height/2) / rect.height) * textureHeight;
            DrawCircle((int)px, (int)py);
            texture.Apply();
        }
        if (isErasing) 
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.rectTransform, Input.mousePosition, Camera.main, out mousePos);
            Rect rect = canvas.rectTransform.rect;
            float px = ((mousePos.x + rect.width / 2) / rect.width) * textureWidth;
            float py = ((mousePos.y + rect.height / 2) / rect.height) * textureHeight;
            DrawCircle((int)px, (int)py);
            texture.Apply();

        }
    }
    private void StartPainting()
    {
        isPainting = true;
    }
    private void ContinuePainting()
    {
        if (!isPainting) return;
    }
    private void StopPainting()
    {
        isPainting = false;
    }
    private void StartErasing()
    {
        isErasing = true;
    }
    private void ContinueErasing()
    {
        if (!isErasing) return;
    }
    private void StopErasing()
    {
        isErasing = false;
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
                    if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
                    {
                        texture.SetPixel(px, py, paintColor);
                    }
                }
            }
        }
    }
    
    public void ClearCanvas()
    {
        try
        {
            Debug.Log("[PaintScript] Clearing canvas...");
            
            // Create a blank white canvas
            Color[] fillColor = new Color[textureWidth * textureHeight];
            for (int i = 0; i < fillColor.Length; i++)
            {
                fillColor[i] = Color.white;
            }
            
            // Apply the blank pixels to the texture
            if (texture != null)
            {
                texture.SetPixels(fillColor);
                texture.Apply();
                Debug.Log("[PaintScript] Canvas cleared successfully");
            }
            else
            {
                Debug.LogError("[PaintScript] Cannot clear canvas - texture is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PaintScript] Error clearing canvas: {e.Message}");
        }
    }



    public void BrushSlider(float newSize)
    {
        brushSize = Mathf.Clamp(Mathf.RoundToInt(newSize), 1, 100);
    }
}
