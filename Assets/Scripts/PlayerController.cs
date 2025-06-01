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
    public Color[] availableColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.black
    };
    
    // Current painting state
    private Texture2D paintingTexture;
    private Vector2Int lastPixelPosition;
    // isPainting moved to PaintScript
    
    // Current submission
    private PaintingSubmission currentSubmission;

    private PaintScript paint;
    private void Start()
    {
        paint = FindFirstObjectByType<PaintScript>();
        
        // Initialize with first color
        if (availableColors.Length > 0) {
            currentColor = availableColors[0];
            // Set initial color in PaintScript too
            if (paint != null) {
                paint.paintColor = currentColor;
            }
        }
        
        Debug.Log("[PlayerController] Initialized with PaintScript reference: " + (paint != null));
    }
    
    // Removed InitializePaintingCanvas as it's now handled by PaintScript
    
    private void Update()
    {
        
        // Color selection with number keys
        for (int i = 0; i < Mathf.Min(availableColors.Length, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                currentColor = availableColors[i];
                Debug.Log(currentColor);
                paint.paintColor = availableColors[i];
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
    
    // Removed painting functionality as it's now handled by PaintScript
    // These methods weren't being used anymore
    
    public void ClearCanvas()
    {
        try
        {
            Debug.Log("[PlayerController] Delegating canvas clearing to PaintScript");
            
            // Simply use PaintScript's clearing function instead
            if (paint != null)
            {
                paint.ClearCanvas();
                Debug.Log("[PlayerController] Canvas cleared via PaintScript");
            }
            else
            {
                Debug.LogError("[PlayerController] Cannot clear canvas - PaintScript reference is null");
            }
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
            
            // Clear for next painting (we're already using PaintScript for this)
            Debug.Log("[PlayerController] Clearing canvas after submission");
            ClearCanvas(); // This method now delegates to PaintScript anyway
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
        paint.paintColor = currentColor;
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
