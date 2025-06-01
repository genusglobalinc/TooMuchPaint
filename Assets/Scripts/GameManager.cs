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
    public Image paintMeterFill; // dark bar that shrinks
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
    public Camera mainCamera;      // Main/menu camera
    public Camera paintingCamera;  // Camera for painting view
    
    // Game state
    private float currentPaint;
    private float currentTime;
    private int score;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private float originalCameraFOV;
    private bool isGameActive;
    private float paintMeterFillFullHeight;
    
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
        try
        {
            // Save the original camera position, rotation and field of view
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                originalCameraPosition = mainCam.transform.position;
                originalCameraRotation = mainCam.transform.rotation;
                originalCameraFOV = mainCam.fieldOfView;
                Debug.Log("[GameManager] Saved original camera settings");
            }
            
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
            
            // Update timer
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
            
            // SHRINK paintMeterFill vertically (height) based on remaining time
            if (paintMeterFill != null)
            {
                RectTransform rt = paintMeterFill.rectTransform;
                if (paintMeterFillFullHeight <= 0f)
                    paintMeterFillFullHeight = rt.sizeDelta.y;
                float pct = Mathf.Clamp01(currentTime / timePerCustomer); // 1 full time, 0 no time
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, paintMeterFillFullHeight * pct);
            }
            
            // Check game over conditions
            if (currentTime <= 0)
            {
                // Timer reached zero - game over!
                GameOver();
            }
            else if (currentPaint >= maxPaint)
            {
                // Paint overflow - game over!
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
            
            // Reset game state variables
            currentPaint = maxPaint / 2f; // Start with half paint
            score = 0;
            isGameActive = true;
            
            // Initialize score display
            if (scoreText != null) {
                scoreText.text = "Score: 0";
            } else {
                Debug.LogError("[GameManager] scoreText is null!");
            }
            
            // Cache original height & lock pivot/anchors so it shrinks to the bottom
            if (paintMeterFill != null)
            {
                RectTransform rt = paintMeterFill.rectTransform;
                paintMeterFillFullHeight = rt.sizeDelta.y;
                rt.pivot = new Vector2(0f, 0f); // lock bottom-left corner
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f); // same corner anchored
            }
            
            Debug.Log($"[GameManager] Initial paint set to {currentPaint:F2}/{maxPaint}, score reset to 0");
            
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
            
            // Switch to painting camera
            SwitchToPaintingCamera();
            
            SpawnNewCustomer();
            Debug.Log("[GameManager] Game successfully started");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error starting game: {e.Message}");
        }
    }
    
    // Create a UI camera setup to focus on the painting canvas
    public void SwitchToPaintingCamera()
    {
        try
        {
            // Get reference to the main camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("[GameManager] No main camera found in the scene!");
                return;
            }

            // Find the painting canvas
            Canvas paintingCanvas = null;
            RawImage paintingImage = null;
            
            // Try to find through player controller first
            if (player != null)
            {
                paintingCanvas = player.paintingCanvas;
                // Get RawImage or cast from Image if needed
                if (player.paintingImage != null) {
                    paintingImage = player.paintingImage.GetComponent<RawImage>();
                    if (paintingImage == null) {
                        Debug.Log("[GameManager] paintingImage is not a RawImage, will find it another way");
                    }
                }
            }
            
            // Alternative way - find PaintScript
            if (paintingCanvas == null)
            {
                PaintScript paintScript = FindFirstObjectByType<PaintScript>();
                if (paintScript != null)
                {
                    paintingCanvas = paintScript.GetComponentInParent<Canvas>();
                    // Try to get the RawImage component directly from the PaintScript
                    paintingImage = paintScript.GetComponent<RawImage>();
                    
                    // If not found, try other methods
                    if (paintingImage == null) {
                        paintingImage = paintScript.gameObject.GetComponentInChildren<RawImage>();
                    }
                }
            }
            
            if (paintingCanvas == null)
            {
                Debug.LogWarning("[GameManager] Could not find painting canvas in the scene");
                return;
            }
            
            // Now we need to adjust the scene to make the painting canvas prominent
            // For Screen Space - Overlay canvas, we need to hide other UI and focus attention
            
            // 1. Hide all UI except the painting canvas and necessary gameplay elements
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas != paintingCanvas && canvas != paintingCanvas.rootCanvas && !canvas.name.Contains("gameUI"))
                {
                    canvas.gameObject.SetActive(false);
                }
            }
            
            // 2. Make a very subtle camera movement to focus on the easel
            // We'll just use a slight field of view change without moving the camera position
            StartCoroutine(SubtleFocusEffect(mainCam, 0.5f));
            
            // 3. Highlight the painting canvas if possible
            if (paintingImage != null)
            {
                // Make the painting image more prominent
                RectTransform rt = paintingImage.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Save original size
                    Vector2 originalSize = rt.sizeDelta;
                    
                    // Slightly enlarge the painting canvas for emphasis
                    StartCoroutine(ScaleRectTransform(rt, originalSize, originalSize * 1.05f, 0.5f));
                }
            }

            Debug.Log("[GameManager] Focused view on painting canvas");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error focusing on painting canvas: {e.Message}");
        }
    }
    
    // Smoothly move the camera to a new position
    private IEnumerator MoveCameraToPosition(Camera camera, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = camera.transform.position;
        float time = 0;
        
        while (time < duration)
        {
            camera.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end exactly at the target position
        camera.transform.position = targetPosition;
    }
    
    // Creates a subtle focusing effect with minimal camera movement
    private IEnumerator SubtleFocusEffect(Camera camera, float duration)
    {
        // Store original values to return to later
        float startFieldOfView = camera.fieldOfView;
        // Just a very slight zoom - only 5% change
        float targetFieldOfView = startFieldOfView * 0.95f;
        float time = 0;
        
        while (time < duration)
        {
            float t = time / duration;
            // Use easing function for smoother transition
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            // ONLY adjust the field of view, don't move the camera position at all
            camera.fieldOfView = Mathf.Lerp(startFieldOfView, targetFieldOfView, smoothT);
            
            time += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at the exact target field of view
        camera.fieldOfView = targetFieldOfView;
    }
    
    // Smoothly scale a RectTransform between two sizes
    private IEnumerator ScaleRectTransform(RectTransform rt, Vector2 startSize, Vector2 targetSize, float duration)
    {
        float time = 0;
        
        while (time < duration)
        {
            float t = time / duration;
            rt.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
            time += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at exact target size
        rt.sizeDelta = targetSize;
    }
    
    // Move the camera back to its original position
    public void SwitchToMainCamera()
    {
        try
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("[GameManager] No main camera found in the scene!");
                return;
            }
            
            // Simply restore the original field of view with a smooth transition
            StartCoroutine(RestoreFieldOfView(mainCam, 0.5f));

            // Show any hidden UI elements again
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in allCanvases)
            {
                canvas.gameObject.SetActive(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Error returning to main camera: {e.Message}");
        }
    }
    
    // Smoothly restore the camera's original field of view
    private IEnumerator RestoreFieldOfView(Camera camera, float duration)
    {
        // Get the current field of view as starting point
        float startFieldOfView = camera.fieldOfView;
        float time = 0;
        
        while (time < duration)
        {
            float t = time / duration;
            // Use easing function for smoother transition
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            // Restore the original field of view that we saved at start
            camera.fieldOfView = Mathf.Lerp(startFieldOfView, originalCameraFOV, smoothT);
            
            time += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at the exact original field of view
        camera.fieldOfView = originalCameraFOV;
    }
    
    // Smoothly move and rotate the camera to a new position and rotation
    private IEnumerator MoveCameraToPositionAndRotation(Camera camera, Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        Vector3 startPosition = camera.transform.position;
        Quaternion startRotation = camera.transform.rotation;
        float time = 0;
        
        while (time < duration)
        {
            camera.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            camera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end exactly at the target position and rotation
        camera.transform.position = targetPosition;
        camera.transform.rotation = targetRotation;
    }
    
    public void SpawnNewCustomer()
    {
        try
        {
            Debug.Log("[GameManager] Spawning new customer...");
            
            // Don't reset timer for new customer, just update UI
            UpdateTimerUI();
            Debug.Log($"[GameManager] Current timer: {currentTime} seconds");
            
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
        // ULTRA SIMPLE - ADD TIME, ADD SCORE, UPDATE UI
        // No conditions, no complexity
        
        // 1. ADD ONE POINT
        score += 1;
        
        // 2. DIRECTLY UPDATE SCORE TEXT
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
            Debug.Log("SCORE UPDATED TO: " + score);
        }
        
        // 3. ADD 5 SECONDS TO TIMER
        currentTime += 5.0f;
        if (currentTime > timePerCustomer)
            currentTime = timePerCustomer;
        
        // 4. GET NEW CUSTOMER
        SpawnNewCustomer();
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
            
            // Update final score on game over screen
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {score}";
                Debug.Log($"[GameManager] Game ended with final score: {score}");
            }
            else
            {
                // Try to find the text component if reference is missing
                TextMeshProUGUI[] texts = gameOverScreen.GetComponentsInChildren<TextMeshProUGUI>();
                bool foundAndUpdated = false;
                
                foreach (TextMeshProUGUI text in texts)
                {
                    if (text.name.ToLower().Contains("score") || text.text.ToLower().Contains("score"))
                    {
                        text.text = $"Final Score: {score}";
                        foundAndUpdated = true;
                        Debug.Log($"[GameManager] Found and updated score text: {text.name}");
                        break;
                    }
                }
                
                if (!foundAndUpdated)
                {
                    Debug.LogError("[GameManager] Could not find any score text on game over screen");
                }
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
