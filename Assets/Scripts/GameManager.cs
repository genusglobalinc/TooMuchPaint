using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("UI References")]
    public GameObject startScreen;
    public GameObject gameOverScreen;
    public GameObject gameUI;
    public Image paintMeter;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI finalScoreText; // Text to display final score on game over screen
    
    [Header("Game Settings")]
    public float maxPaint = 100f;
    public float paintLossPerSecond = 1f;
    public float timePerCustomer = 60f;
    public float paintPenaltyPerMistake = 10f;
    public float paintRewardPerSuccess = 15f;
    
    [Header("References")]
    public PlayerController player;
    public NPCController currentNPC;
    
    // Game state
    private float currentPaint;
    private int score;
    private float currentTime;
    private bool isGameActive;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Show start screen when game begins
        try
        {
            Debug.Log("[GameManager] Initializing game and showing start screen");
            
            if (startScreen != null)
                startScreen.SetActive(true);
            else
                Debug.LogError("[GameManager] startScreen is null during initialization");
                
            if (gameOverScreen != null)
                gameOverScreen.SetActive(false);
            else
                Debug.LogError("[GameManager] gameOverScreen is null during initialization");
                
            if (gameUI != null)
                gameUI.SetActive(false);
            else
                Debug.LogError("[GameManager] gameUI is null during initialization");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error in Start: {e.Message}");
        }
    }
    
    private void Update()
    {
        try
        {
            if (!isGameActive) return;
            
            // Update paint meter
            float oldPaint = currentPaint;
            currentPaint -= paintLossPerSecond * Time.deltaTime;
            
            // Removed excessive paint level debug logs
            
            if (paintMeter != null)
            {
                paintMeter.fillAmount = currentPaint / maxPaint;
            }
            else
            {
                Debug.LogError("[GameManager] paintMeter is null");
            }
            
            // Update timer
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
            
            // Check game over conditions
            if (currentTime <= 0)
            {
                Debug.Log("[GameManager] Timer reached zero - game over!");
                GameOver();
            }
            else if (currentPaint >= maxPaint)
            {
                Debug.Log($"[GameManager] Paint overflow ({currentPaint:F2}/{maxPaint}) - game over!");
                GameOver();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error in Update: {e.Message}");
        }
    }
    
    public void StartGame()
    {
        try
        {
            Debug.Log("[GameManager] Starting new game...");
            
            currentPaint = maxPaint / 2f; // Start with half paint
            score = 0;
            isGameActive = true;
            
            Debug.Log($"[GameManager] Initial paint set to {currentPaint:F2}/{maxPaint}");
            
            if (startScreen != null)
                startScreen.SetActive(false);
            else
                Debug.LogError("[GameManager] startScreen is null");
                
            if (gameOverScreen != null)
                gameOverScreen.SetActive(false);
            else
                Debug.LogError("[GameManager] gameOverScreen is null");
                
            if (gameUI != null)
                gameUI.SetActive(true);
            else
                Debug.LogError("[GameManager] gameUI is null");
            
            Debug.Log("[GameManager] UI panels set up for gameplay");
            
            // Reset and start the timer
            currentTime = timePerCustomer;
            UpdateTimerUI();
            
            SpawnNewCustomer();
            Debug.Log("[GameManager] Game successfully started");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error starting game: {e.Message}");
        }
    }
    
    public void SpawnNewCustomer()
    {
        try
        {
            Debug.Log("[GameManager] Spawning new customer...");
            
            // Reset timer for new customer
            currentTime = timePerCustomer;
            UpdateTimerUI();
            Debug.Log($"[GameManager] Timer reset to {timePerCustomer} seconds");
            
            if (currentNPC != null)
            {
                // Make sure NPC game object is active
                currentNPC.gameObject.SetActive(true);
                
                // Generate new attributes
                currentNPC.GenerateRandomAttributes();
                currentNPC.ShowAttributes();
                
                // Ensure sprite and text are visible
                if (currentNPC.GetComponent<SpriteRenderer>() != null)
                {
                    currentNPC.GetComponent<SpriteRenderer>().enabled = true;
                }
                
                Debug.Log("[GameManager] NPC fully activated with new attributes");
            }
            else
            {
                Debug.LogError("[GameManager] Cannot spawn new customer - currentNPC is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error spawning new customer: {e.Message}");
        }
    }
    
    public void SubmitPainting(PaintingSubmission submission)
    {
        try
        {
            Debug.Log("[GameManager] Processing painting submission...");
            
            if (!isGameActive)
            {
                Debug.LogWarning("[GameManager] Cannot process submission - game is not active");
                return;
            }
            
            if (submission == null)
            {
                Debug.LogError("[GameManager] Submission is null");
                return;
            }
            
            if (currentNPC == null)
            {
                Debug.LogError("[GameManager] Cannot compare submission - currentNPC is null");
                return;
            }
            
            // Compare submission with current NPC's attributes
            bool isMatch = currentNPC.CompareWithSubmission(submission);
            Debug.Log($"[GameManager] Submission match result: {isMatch}");
            
            if (isMatch)
            {
                // Success - ADDS paint (risk of overflow)
                float oldPaint = currentPaint;
                currentPaint = Mathf.Min(maxPaint, currentPaint + paintRewardPerSuccess);
                score += 100;
                scoreText.text = $"Score: {score}";
                
                Debug.Log($"[GameManager] Success! Paint increased from {oldPaint:F2} to {currentPaint:F2}. New score: {score}");
            }
            else
            {
                // Mistake - REDUCES paint (good thing)
                float oldPaint = currentPaint;
                currentPaint = Mathf.Max(0, currentPaint - paintPenaltyPerMistake);
                
                Debug.Log($"[GameManager] Mistake! Paint decreased from {oldPaint:F2} to {currentPaint:F2}");
            }
            
            // Get next customer - ONLY after submission, not when clearing
            Debug.Log("[GameManager] Spawning new NPC after submission...");
            SpawnNewCustomer();
            
            // Force a repaint
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error processing submission: {e.Message}");
        }
    }
    
    public void CustomerRejected()
    {
        // Penalize for not completing in time
        currentPaint -= paintPenaltyPerMistake / 2f;
        SpawnNewCustomer();
    }
    
    private void UpdateTimerUI()
    {
        try
        {
            int seconds = Mathf.CeilToInt(currentTime);
            
            if (timerText != null)
            {
                timerText.text = $"Time: {seconds}s";
                
                // Add warning colors when time is running low
                if (seconds <= 10)
                {
                    timerText.color = Color.red;
                    // Log only occasionally to avoid spam
                    if (seconds % 2 == 0) 
                        Debug.Log($"[GameManager] Warning: Low time - {seconds}s");
                }
                else
                {
                    timerText.color = Color.white;
                }
                
                // Removed excessive timer debug logs
            }
            else
            {
                Debug.LogError("[GameManager] timerText is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error updating timer UI: {e.Message}");
        }
    }
    
    public void GameOver()
    {
        try
        {
            Debug.Log("[GameManager] Game Over triggered!");
            isGameActive = false;
            
            // Show game over screen
            if (gameUI != null)
                gameUI.SetActive(false);
            else
                Debug.LogError("[GameManager] gameUI is null during GameOver");
                
            if (gameOverScreen != null)
                gameOverScreen.SetActive(true);
            else
                Debug.LogError("[GameManager] gameOverScreen is null during GameOver");
            
            // Update final score
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {score}";
                Debug.Log($"[GameManager] Game ended with final score: {score}");
            }
            else
            {
                // Still log the score even if UI element is missing
                Debug.LogError("[GameManager] finalScoreText is null during GameOver");
                Debug.Log($"[GameManager] Game ended with final score: {score} (not displayed in UI)");
            }
            
            Debug.Log("[GameManager] Game Over screen displayed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error during GameOver: {e.Message}");
        }
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
