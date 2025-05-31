using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Canvas paintingCanvas;
    public Image paintingImage;
    public Camera paintingCamera;
    public LayerMask canvasLayer;
    
    [Header("Painting Settings")]
    public float brushSize = 0.1f;
    public Color currentColor = Color.red;
    public float paintConsumptionRate = 0.1f;
    
    [Header("Color Palette")]
    public Color[] availableColors;
    
    // Current painting state
    private Texture2D paintingTexture;
    private Vector2Int lastPixelPosition;
    private bool isPainting = false;
    
    // Current submission
    private PaintingSubmission currentSubmission;
    
    private void Start()
    {
        InitializePaintingCanvas();
        
        // Initialize with first color
        if (availableColors.Length > 0)
            currentColor = availableColors[0];
    }
    
    private void InitializePaintingCanvas()
    {
        // Create a new texture for painting
        int width = 256;
        int height = 256;
        paintingTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        
        // Fill with transparent color
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        paintingTexture.SetPixels(pixels);
        paintingTexture.Apply();
        
        // Apply to the UI Image
        paintingImage.sprite = Sprite.Create(paintingTexture, new Rect(0, 0, width, height), Vector2.one * 0.5f);
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
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
        
        // Color selection with number keys
        for (int i = 0; i < Mathf.Min(availableColors.Length, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                currentColor = availableColors[i];
            }
        }
        
        // Submit painting with Enter key
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SubmitPainting();
        }
        
        // Clear canvas with C key
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearCanvas();
        }
    }
    
    private void StartPainting()
    {
        Vector2 localPoint;
        if (IsPointerOverCanvas(out localPoint))
        {
            isPainting = true;
            PaintAtPosition(localPoint);
        }
    }
    
    private void ContinuePainting()
    {
        if (!isPainting) return;
        
        Vector2 localPoint;
        if (IsPointerOverCanvas(out localPoint))
        {
            PaintAtPosition(localPoint);
        }
    }
    
    private void StopPainting()
    {
        isPainting = false;
    }
    
    private bool IsPointerOverCanvas(out Vector2 localPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            paintingCanvas.transform as RectTransform, 
            Input.mousePosition, 
            paintingCamera, 
            out localPoint);
            
        Rect rect = (paintingCanvas.transform as RectTransform).rect;
        return rect.Contains(localPoint);
    }
    
    private void PaintAtPosition(Vector2 localPoint)
    {
        // Convert local point to texture coordinates
        Rect rect = (paintingCanvas.transform as RectTransform).rect;
        float x = (localPoint.x + rect.width * 0.5f) / rect.width * paintingTexture.width;
        float y = (localPoint.y + rect.height * 0.5f) / rect.height * paintingTexture.height;
        
        // Paint a circle at the current position
        int radius = Mathf.RoundToInt(brushSize * paintingTexture.width * 0.1f);
        int xStart = Mathf.RoundToInt(x - radius);
        int yStart = Mathf.RoundToInt(y - radius);
        
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                int px = xStart + i;
                int py = yStart + j;
                
                // Check if within circle
                if (i * i + j * j <= radius * radius && 
                    px >= 0 && px < paintingTexture.width && 
                    py >= 0 && py < paintingTexture.height)
                {
                    paintingTexture.SetPixel(px, py, currentColor);
                }
            }
        }
        
        paintingTexture.Apply();
    }
    
    public void ClearCanvas()
    {
        try
        {
            Debug.Log("[PlayerController] Clearing canvas...");
            
            if (paintingTexture == null)
            {
                Debug.LogError("[PlayerController] Cannot clear canvas - paintingTexture is null");
                return;
            }
            
            Color[] pixels = paintingTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white; // Use white instead of clear for consistent behavior
            }
            paintingTexture.SetPixels(pixels);
            paintingTexture.Apply();
            
            Debug.Log("[PlayerController] Canvas cleared successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerController] Error clearing canvas: {e.Message}");
        }
    }
    
    public void SubmitPainting()
    {
        try
        {
            Debug.Log("[PlayerController] Submitting painting...");
            
            // Create a submission with the current painting
            currentSubmission = new PaintingSubmission
            {
                emotion = "Happy", // This would be set by the player's painting
                hairColor = currentColor, // This would be determined by the painting
                eyeColor = currentColor, // This would be determined by the painting
                accessory = "None" // This would be determined by the painting
            };
            
            Debug.Log($"[PlayerController] Created submission with emotion:{currentSubmission.emotion}, " + 
                      $"hair:{currentSubmission.hairColor}, eyes:{currentSubmission.eyeColor}, " + 
                      $"accessory:{currentSubmission.accessory}");
            
            // Notify the GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SubmitPainting(currentSubmission);
                Debug.Log("[PlayerController] Submission sent to GameManager");
            }
            else
            {
                Debug.LogError("[PlayerController] Cannot submit painting - GameManager.Instance is null");
            }
            
            // Clear for next painting
            Debug.Log("[PlayerController] Clearing canvas after submission");
            ClearCanvas();
            
            // Also try to clear using PaintScript directly as a backup
            PaintScript paintScript = FindFirstObjectByType<PaintScript>();
            if (paintScript != null)
            {
                Debug.Log("[PlayerController] Also clearing via PaintScript for reliability");
                paintScript.ClearCanvas();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerController] Error submitting painting: {e.Message}");
        }
    }
    
    public void SetBrushSize(float size)
    {
        brushSize = Mathf.Clamp(size, 0.1f, 2f);
    }
    
    public void SetColor(Color color)
    {
        currentColor = color;
    }
}

[System.Serializable]
public class PaintingSubmission
{
    public string emotion;
    public Color hairColor;
    public Color eyeColor;
    public string accessory;
    // Add more attributes as needed
}
