using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text bestScoreText;
    

    private const float HitCooldown = 1.5f;
    private const float UnitsPerPoint = 10f;
    private const string BestScoreKey = "BestScore";
    private const float FishDuration = 5f;
    private const int FishMultiplier = 2;
    private const float ShieldDuration = 5f;
    private int lives = 3;
    private float nextDamageTime;
    private float startZ;
    private int score;
    private int bestScore;
    private bool isGameOver;
    private int distanceSteps;
    private float multiplierEndTime;
    private bool multiplierWasActive;
    private float shieldEndTime;
    private bool shieldWasActive;

    private bool IsMultiplierActive => Time.time < multiplierEndTime;
    private bool IsShieldActive => Time.time < shieldEndTime;
    
    

    private void Start()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        startZ = transform.position.z;
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        UpdateBestScoreText();
        UpdateLivesText();
        UpdateScoreText();
    }

    private void Update()
    {
        if (isGameOver)
            return;

        int steps = Mathf.FloorToInt((transform.position.z - startZ) / UnitsPerPoint);
        if (steps > distanceSteps)
        {
            int multiplier = IsMultiplierActive ? FishMultiplier : 1;
            score += (steps - distanceSteps) * multiplier;
            distanceSteps = steps;
            UpdateScoreText();
        }

        if (IsMultiplierActive != multiplierWasActive)
        {
            multiplierWasActive = IsMultiplierActive;
            UpdateScoreText();
        }
        
        if (IsShieldActive != shieldWasActive)
        {
            shieldWasActive = IsShieldActive;
            UpdateLivesText();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Collider obstacle = collision.collider;
        if (isGameOver || !obstacle.CompareTag("Obstacle"))
            return;

        // Let the cat pass the obstacle after contact.
        obstacle.enabled = false;
        TakeHit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGameOver)
            return;

        if (other.CompareTag("Fish"))
        {
            other.gameObject.SetActive(false);
            multiplierEndTime = Time.time + FishDuration;
        }
        else if (other.CompareTag("Shield"))
        {
            other.gameObject.SetActive(false);
            shieldEndTime = Time.time + ShieldDuration;
        }
        else if (other.CompareTag("Bomb"))
        {
            other.gameObject.SetActive(false);
            TakeHit();
        }
    }

        private void TakeHit()
    {
        if (IsShieldActive || Time.time < nextDamageTime)
            return;

        lives--;
        nextDamageTime = Time.time + HitCooldown;
        UpdateLivesText();

        if (lives == 0)
        {
            isGameOver = true;
            SaveBestScore();
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    
    private void UpdateLivesText()
    {
        string shield = IsShieldActive ? "  Shield" : "";
        livesText.text = $"Lives: {lives}{shield}";
    }

    private void UpdateScoreText()
    {
        string bonus = IsMultiplierActive ? $"  x{FishMultiplier}" : "";
        scoreText.text = $"Score: {score}{bonus}";
    }
    
    private void SaveBestScore()
    {
        if (score <= bestScore)
            return;

        bestScore = score;
        PlayerPrefs.SetInt(BestScoreKey, bestScore);
        PlayerPrefs.Save();
        UpdateBestScoreText();
    }

    private void UpdateBestScoreText()
    {
        bestScoreText.text = $"Best: {bestScore}";
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}