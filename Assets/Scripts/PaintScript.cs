using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Video;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro; // Add TextMeshPro namespace

public class PaintScript : MonoBehaviour
{
    private RawImage canvas;
    public int brushSize = 10;
    
    public int textureWidth = 512;
    public int textureHeight = 512;
    private Texture2D texture;

    // Color selection system
    [Header("Color Options")]
    public Color[] availableColors = new Color[] {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.black,
        new Color(1f, 0.5f, 0f), // Orange
        new Color(0.5f, 0f, 0.5f), // Purple
        new Color(1f, 0.75f, 0.8f), // Pink
        new Color(0.5f, 0.25f, 0f)  // Brown
    };
    public Color paintColor = Color.red;
    private int currentColorIndex = 0;
    
    // Painting state
    private bool isPainting = false;
    private bool isErasing = false;
    private Color previousColor;
    
    // Reference to PlayerController for color synchronization
    private PlayerController playerController;

    public Slider brushSlider;
    void Start()
    {
        canvas = GetComponent<RawImage>();
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        ClearCanvas(); // Use our clear method to initialize
        canvas.texture = texture;
        
        // Get PlayerController reference
        playerController = FindFirstObjectByType<PlayerController>();
        
        // Find and connect to brush size slider if it exists
        FindAndConnectBrushSlider();
        
        // Only log instructions for color keys
        Debug.Log("[PaintScript] Use number keys 1-9 to change colors");
    }
    
    // Automatically find and connect to a brush size slider in the scene
    private void FindAndConnectBrushSlider()
    {
        try
        {
            // Look for sliders with common names
            string[] possibleNames = new string[] {
                "BrushSizeSlider", "BrushSlider", "SizeSlider", "Slider"
            };
            
            Slider foundSlider = null;
            
            // Try to find by name first
            foreach (string name in possibleNames)
            {
                GameObject sliderObj = GameObject.Find(name);
                if (sliderObj != null)
                {
                    foundSlider = sliderObj.GetComponent<Slider>();
                    if (foundSlider != null)
                    {
                        Debug.Log($"[PaintScript] Found slider by name: {name}");
                        break;
                    }
                }
            }
            
            // If not found by name, find any slider in the scene
            if (foundSlider == null)
            {
                foundSlider = FindFirstObjectByType<Slider>();
                Debug.Log($"[PaintScript] Found slider by type search: {(foundSlider != null ? foundSlider.name : "none")}");
            }
            
            // Connect to the slider if found
            if (foundSlider != null)
            {
                brushSlider = foundSlider;
                foundSlider.onValueChanged.AddListener(BrushSlider);
                
                // Set initial slider value to match our brush size
                foundSlider.value = brushSize;
                
                Debug.Log("[PaintScript] Successfully connected to brush size slider: " + foundSlider.name);
            }
            else
            {
                Debug.LogWarning("[PaintScript] No brush size slider found in the scene");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PaintScript] Error connecting to brush slider: {e.Message}");
        }
    }
    
    // Dynamic UI creation removed - using keyboard shortcuts instead
    void Update()
    {
        // Handle color selection with number keys 1-9
        for (int i = 0; i < Mathf.Min(availableColors.Length, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetColorByIndex(i);
            }
        }

        // Painting with left mouse button
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
        
        // Erasing with right mouse button
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
        
        Vector2 mousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.rectTransform, Input.mousePosition, Camera.main, out mousePos))
        {
            Rect rect = canvas.rectTransform.rect;
            float px = ((mousePos.x + rect.width/2) / rect.width) * textureWidth;
            float py = ((mousePos.y + rect.height/2) / rect.height) * textureHeight;
            
            // Check if the point is within the canvas boundaries before drawing
            if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
            {
                DrawCircle((int)px, (int)py);
                texture.Apply();
            }
        }
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
    
    // New color selection methods
    public void SetColorByIndex(int index)
    {
        if (index >= 0 && index < availableColors.Length)
        {
            currentColorIndex = index;
            paintColor = availableColors[index];
            // Color changed silently
            
            // If this color is being used for erasing, update the previous color instead
            if (isErasing)
            {
                previousColor = paintColor;
            }
            else
            {
                // Also update PlayerController color for consistency
                if (playerController != null)
                {
                    playerController.SetColor(paintColor);
                    Debug.Log($"[PaintScript] Synchronized color with PlayerController");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[PaintScript] Invalid color index: {index}");
        }
    }
    
    // Get current color index
    private int GetCurrentColorIndex()
    {
        return currentColorIndex;
    }



    public void BrushSlider(float newSize)
    {
        brushSize = Mathf.Clamp(Mathf.RoundToInt(newSize), 1, 100);
        Debug.Log($"[PaintScript] Brush size set to {brushSize}");
    }
}
