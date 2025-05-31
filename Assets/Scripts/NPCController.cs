using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCAttributes
{
    public string emotion = "Happy";
    public Color hairColor = Color.black;
    public Color eyeColor = Color.blue;
    public string accessory = "None";
    // Add more attributes as needed
}

public class NPCController : MonoBehaviour
{
    [Header("References")]
    public Renderer faceRenderer; // For showing the NPC's face
    public TextMesh attributeText; // For displaying attributes (temporary)
    
    [Header("Settings")]
    public float attributeDisplayTime = 5f;
    
    // Current attributes
    private NPCAttributes currentAttributes;
    
    // Possible values for attributes
    private string[] possibleEmotions = { "Happy", "Sad", "Angry", "Surprised", "Neutral" };
    private string[] possibleAccessories = { "None", "Glasses", "Hat", "Earrings", "Scarf" };
    
    private void Start()
    {
        // Hide attributes initially
        if (attributeText != null)
            attributeText.gameObject.SetActive(false);
    }
    
    public void GenerateRandomAttributes()
    {
        try
        {
            // Select random attributes
            string emotion = possibleEmotions[Random.Range(0, possibleEmotions.Length)];
            Color hairColor = new Color(Random.value, Random.value, Random.value);
            Color eyeColor = new Color(Random.value, Random.value, Random.value);
            string accessory = possibleAccessories[Random.Range(0, possibleAccessories.Length)];
            
            // Create the attributes object
            currentAttributes = new NPCAttributes
            {
                emotion = emotion,
                hairColor = hairColor,
                eyeColor = eyeColor,
                accessory = accessory
            };
            
            // Log the generated attributes
            Debug.Log($"[NPC] Generated new NPC with:\n" +
                      $"  - Emotion: {emotion}\n" +
                      $"  - Hair: {ColorToName(hairColor)} ({hairColor})\n" +
                      $"  - Eyes: {ColorToName(eyeColor)} ({eyeColor})\n" +
                      $"  - Accessory: {accessory}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NPC] Error generating attributes: {e.Message}");
        }
    }
    
    public void ShowAttributes()
    {
        if (currentAttributes == null) return;
        
        // Show the attributes for a limited time (attributes are currently disabled)
        // We removed the isShowingAttributes flag since it's not used elsewhere
        
        // Update the display
        if (attributeText != null)
        {
            attributeText.text = $"Emotion: {currentAttributes.emotion}\n" +
                              $"Hair: {ColorToName(currentAttributes.hairColor)}\n" +
                              $"Eyes: {ColorToName(currentAttributes.eyeColor)}\n" +
                              $"Accessory: {currentAttributes.accessory}";
            
            // Keep attribute text hidden but log the data for debugging
            attributeText.gameObject.SetActive(false);
            Debug.Log($"[NPCController] Generated NPC with attributes: " +
                      $"Emotion={currentAttributes.emotion}, " +
                      $"Hair={ColorToName(currentAttributes.hairColor)}, " +
                      $"Eyes={ColorToName(currentAttributes.eyeColor)}, " +
                      $"Accessory={currentAttributes.accessory}");
            
            // Ensure the NPC sprite is visible but text is hidden
            if (GetComponent<SpriteRenderer>() != null)
            {
                GetComponent<SpriteRenderer>().enabled = true;
                Debug.Log("[NPCController] NPC sprite enabled");
            }
        }
    }
    
    private void HideAttributes()
    {
        // Just hide the attribute text
        if (attributeText != null)
            attributeText.gameObject.SetActive(false);
            
        Debug.Log("[NPCController] Attributes hidden");
    }
    
    public bool CompareWithSubmission(PaintingSubmission submission)
    {
        // Simple comparison - in a real game, you might want to be more sophisticated
        bool emotionMatch = submission.emotion == currentAttributes.emotion;
        bool hairMatch = ColorsAreClose(submission.hairColor, currentAttributes.hairColor, 0.2f);
        bool eyeMatch = ColorsAreClose(submission.eyeColor, currentAttributes.eyeColor, 0.2f);
        bool accessoryMatch = submission.accessory == currentAttributes.accessory;
        
        // For now, require all attributes to match
        return emotionMatch && hairMatch && eyeMatch && accessoryMatch;
    }
    
    private bool ColorsAreClose(Color a, Color b, float threshold)
    {
        return Vector4.Distance((Vector4)a, (Vector4)b) < threshold;
    }
    
    private string ColorToName(Color color)
    {
        // Simple color name mapping
        if (color.r > 0.8f && color.g < 0.2f && color.b < 0.2f) return "Red";
        if (color.g > 0.8f && color.r < 0.2f && color.b < 0.2f) return "Green";
        if (color.b > 0.8f && color.r < 0.2f && color.g < 0.2f) return "Blue";
        if (color.r > 0.8f && color.g > 0.8f && color.b < 0.2f) return "Yellow";
        if (color.r > 0.8f && color.b > 0.8f && color.g < 0.2f) return "Purple";
        if (color.g > 0.8f && color.b > 0.8f && color.r < 0.2f) return "Cyan";
        if (color.r > 0.8f && color.g > 0.8f && color.b > 0.8f) return "White";
        if (color.r < 0.2f && color.g < 0.2f && color.b < 0.2f) return "Black";
        return "Custom Color";
    }
}
