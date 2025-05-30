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
        ShowStartScreen();
    }
    
    private void Update()
    {
        if (!isGameActive) return;
        
        // Update paint meter
        currentPaint -= paintLossPerSecond * Time.deltaTime;
        paintMeter.fillAmount = currentPaint / maxPaint;
        
        // Update timer
        currentTime -= Time.deltaTime;
        UpdateTimerUI();
        
        // Check game over conditions
        if (currentPaint <= 0)
        {
            GameOver();
        }
        else if (currentTime <= 0)
        {
            // Time's up for current customer
            CustomerRejected();
        }
    }
    
    public void StartGame()
    {
        currentPaint = maxPaint / 2f; // Start with half paint
        score = 0;
        isGameActive = true;
        
        startScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        gameUI.SetActive(true);
        
        SpawnNewCustomer();
    }
    
    public void SpawnNewCustomer()
    {
        // Reset timer for new customer
        currentTime = timePerCustomer;
        UpdateTimerUI();
        
        if (currentNPC != null)
        {
            currentNPC.GenerateRandomAttributes();
            currentNPC.ShowAttributes();
        }
    }
    
    public void SubmitPainting(PaintingSubmission submission)
    {
        if (!isGameActive) return;
        
        // Compare submission with current NPC's attributes
        bool isMatch = currentNPC.CompareWithSubmission(submission);
        
        if (isMatch)
        {
            // Success!
            currentPaint = Mathf.Min(maxPaint, currentPaint + paintRewardPerSuccess);
            score += 100;
            scoreText.text = $"Score: {score}";
        }
        else
        {
            // Mistake!
            currentPaint -= paintPenaltyPerMistake;
        }
        
        // Get next customer
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
        int seconds = Mathf.CeilToInt(currentTime);
        timerText.text = $"Time: {seconds}s";
    }
    
    public void GameOver()
    {
        isGameActive = false;
        gameUI.SetActive(false);
        gameOverScreen.SetActive(true);
    }
    
    public void ShowStartScreen()
    {
        startScreen.SetActive(true);
        gameOverScreen.SetActive(false);
        gameUI.SetActive(false);
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
