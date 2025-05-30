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
    private bool isShowingAttributes = false;
    
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
        currentAttributes = new NPCAttributes
        {
            emotion = possibleEmotions[Random.Range(0, possibleEmotions.Length)],
            hairColor = new Color(Random.value, Random.value, Random.value),
            eyeColor = new Color(Random.value, Random.value, Random.value),
            accessory = possibleAccessories[Random.Range(0, possibleAccessories.Length)]
        };
    }
    
    public void ShowAttributes()
    {
        if (currentAttributes == null) return;
        
        // Show the attributes for a limited time
        isShowingAttributes = true;
        
        // Update the display
        if (attributeText != null)
        {
            attributeText.text = $"Emotion: {currentAttributes.emotion}\n" +
                              $"Hair: {ColorToName(currentAttributes.hairColor)}\n" +
                              $"Eyes: {ColorToName(currentAttributes.eyeColor)}\n" +
                              $"Accessory: {currentAttributes.accessory}";
            
            attributeText.gameObject.SetActive(true);
        }
        
        // Hide after delay
        Invoke("HideAttributes", attributeDisplayTime);
    }
    
    private void HideAttributes()
    {
        isShowingAttributes = false;
        if (attributeText != null)
            attributeText.gameObject.SetActive(false);
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
