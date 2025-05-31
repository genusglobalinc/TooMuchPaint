using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject startScreen;
    public GameObject gameOverScreen;
    public GameObject gameUI;
    
    [Header("Game UI")]
    public Image paintMeter;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI finalScoreText;
    
    [Header("Color Buttons")]
    public ColorButton[] colorButtons;
    
    private PlayerController player;
    
    private void Start()
    {
        // Find the player using newer API
        player = FindFirstObjectByType<PlayerController>();
        Debug.Log(player != null ? "[UIManager] Found PlayerController" : "[UIManager] PlayerController not found");
        
        // Set up color buttons
        if (player != null && colorButtons != null && colorButtons.Length > 0)
        {
            for (int i = 0; i < Mathf.Min(colorButtons.Length, player.availableColors.Length); i++)
            {
                int index = i; // Capture the index for the lambda
                colorButtons[i].button.onClick.AddListener(() => SelectColor(index));
                colorButtons[i].image.color = player.availableColors[i];
            }
        }
    }
    
    public void UpdatePaintMeter(float fillAmount)
    {
        if (paintMeter != null)
        {
            paintMeter.fillAmount = fillAmount;
            // Optionally change color based on fill amount
            paintMeter.color = Color.Lerp(Color.red, Color.green, fillAmount);
        }
    }
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = $"Time: {seconds}s";
        }
    }
    
    public void ShowGameOver(int finalScore)
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {finalScore}";
            }
        }
    }
    
    public void ShowGameUI()
    {
        startScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        gameUI.SetActive(true);
    }
    
    public void ShowStartScreen()
    {
        startScreen.SetActive(true);
        gameOverScreen.SetActive(false);
        gameUI.SetActive(false);
    }
    
    public void OnStartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }
    
    public void OnQuitButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
    
    public void OnSubmitButtonClicked()
    {
        if (player != null)
        {
            player.SubmitPainting();
        }
    }
    
    public void OnClearButtonClicked()
    {
        Debug.Log("[UIManager] Clear button clicked");
        
        if (player != null)
        {
            try
            {
                // Check if the player has a PaintScript component instead of using PlayerController's ClearCanvas
                PaintScript paintScript = FindFirstObjectByType<PaintScript>();
                if (paintScript != null)
                {
                    Debug.Log("[UIManager] Using PaintScript.ClearCanvas()");
                    paintScript.ClearCanvas();
                }
                else
                {
                    Debug.Log("[UIManager] Using PlayerController.ClearCanvas()");
                    player.ClearCanvas();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] Error clearing canvas: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("[UIManager] Cannot clear canvas - player reference is null");
        }
    }
    
    private void SelectColor(int colorIndex)
    {
        if (player != null && colorIndex >= 0 && colorIndex < player.availableColors.Length)
        {
            player.SetColor(player.availableColors[colorIndex]);
            
            // Highlight the selected button
            for (int i = 0; i < colorButtons.Length; i++)
            {
                if (colorButtons[i].highlight != null)
                {
                    colorButtons[i].highlight.SetActive(i == colorIndex);
                }
            }
        }
    }

    public void OnRedButtonPress()
    {
        if (player != null)
        {
            SelectColor(0); 
        }
    }
}

[System.Serializable]
public class ColorButton
{
    public Button button;
    public Image image;
    public GameObject highlight; // Optional highlight for selected color
}
