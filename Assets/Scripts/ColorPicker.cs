using UnityEngine;
using UnityEngine.UI;
using System;

public class ColorPicker : MonoBehaviour
{
    [Header("Color Picker Components")]
    public RawImage colorPickerImage;   // Drag the color wheel texture here
    public RawImage selectedColorPreview; // Visual preview of selected color
    public Texture2D colorWheelTexture;   // The color wheel texture

    // References to other components
    private PlayerController playerController;

    // Currently selected color
    private Color _currentColor = Color.red;
    public Color CurrentColor { 
        get { return _currentColor; } 
        private set {
            _currentColor = value;
            if (selectedColorPreview != null)
                selectedColorPreview.color = _currentColor;
        }
    }

    private void Start()
    {
        if (colorWheelTexture == null && colorPickerImage != null)
        {
            colorWheelTexture = colorPickerImage.texture as Texture2D;
        }

        // Find the player controller
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("[ColorPicker] Could not find PlayerController in scene!");
        }
        else
        {
            Debug.Log("[ColorPicker] Successfully connected to PlayerController");
        }

        // Initialize with red color
        CurrentColor = Color.red;
        if (selectedColorPreview != null)
            selectedColorPreview.color = CurrentColor;
    }

    private void Update()
    {
        // Check if the user clicked on the color picker
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (IsPointerOverImage(colorPickerImage, mousePos))
            {
                SelectColorFromWheel(mousePos);
            }
        }
    }

    private void SelectColorFromWheel(Vector2 mousePos)
    {
        if (colorWheelTexture == null) return;

        // Convert screen position to local position in the color wheel
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            colorPickerImage.rectTransform,
            mousePos,
            null, // No camera for screen space overlay
            out Vector2 localPoint);

        // Convert local point to UV coordinates (0-1 range)
        Vector2 normalizedPoint = new Vector2(
            (localPoint.x / colorPickerImage.rectTransform.rect.width) + 0.5f,
            (localPoint.y / colorPickerImage.rectTransform.rect.height) + 0.5f);

        // Get the pixel coordinate
        int x = Mathf.Clamp(Mathf.RoundToInt(normalizedPoint.x * colorWheelTexture.width), 0, colorWheelTexture.width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(normalizedPoint.y * colorWheelTexture.height), 0, colorWheelTexture.height - 1);

        // Get the color at that position
        try
        {
            Color selectedColor = colorWheelTexture.GetPixel(x, y);

            // If the selected color is too transparent, ignore it (clicked outside the wheel)
            if (selectedColor.a < 0.5f) return;

            // Set the current color and update UI
            CurrentColor = selectedColor;

            // Directly update the player controller
            if (playerController != null)
            {
                playerController.SetColor(CurrentColor);
            }

            Debug.Log($"[ColorPicker] Selected color: {CurrentColor} at position {x},{y}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ColorPicker] Error getting pixel color: {e.Message}");
        }
    }

    // Helper method to check if the pointer is over a UI element
    private bool IsPointerOverImage(RawImage image, Vector2 screenPoint)
    {
        if (image == null) return false;
        
        return RectTransformUtility.RectangleContainsScreenPoint(
            image.rectTransform, 
            screenPoint,
            null); // No camera for screen space overlay
    }

    // Method that can be called directly from UI buttons if needed
    public void SetColor(Color newColor)
    {
        CurrentColor = newColor;
        
        // Directly update the player controller
        if (playerController != null)
        {
            playerController.SetColor(CurrentColor);
        }
    }
}
