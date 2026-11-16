using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject shieldVisual;

    private const float HitCooldown = 1.5f;
    private const float UnitsPerPoint = 10f;
    private const string BestScoreKey = "BestScore";
    private const float FishDuration = 5f;
    private const int FishMultiplier = 2;
    private const float ShieldDuration = 5f;
    private static bool replayRequested;
    private int lives = 3;
    private float nextDamageTime;
    private float startZ;
    private int score;
    private int bestScore;
    private bool runStarted;
    private bool isGameOver;
    private int distanceSteps;
    private float multiplierEndTime;
    private float shieldEndTime;

    private bool IsMultiplierActive => Time.time < multiplierEndTime;
    private bool IsShieldActive => Time.time < shieldEndTime;

    private void Start()
    {
        ui.SetGameOver(false);
        startZ = transform.position.z;
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        ui.SetBestScore(bestScore);
        ui.SetLives(lives);
        ui.SetScore(score);
        RefreshPowerUps();

        if (replayRequested)
        {
            replayRequested = false;
            StartRun();
        }
        else
        {
            player.enabled = false;
            Time.timeScale = 0f;
            ui.SetStartScreen(true);
        }
    }

    public void StartRun()
    {
        if (runStarted)
            return;

        runStarted = true;
        ui.SetStartScreen(false);
        player.enabled = true;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (!runStarted || isGameOver)
            return;

        int steps = Mathf.FloorToInt((transform.position.z - startZ) / UnitsPerPoint);
        if (steps > distanceSteps)
        {
            int multiplier = IsMultiplierActive ? FishMultiplier : 1;
            score += (steps - distanceSteps) * multiplier;
            distanceSteps = steps;
            ui.SetScore(score);
        }

        RefreshPowerUps();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider obstacle = collision.collider;
        if (!runStarted || isGameOver || !obstacle.CompareTag("Obstacle"))
            return;

        // Let the cat pass the obstacle after contact.
        obstacle.enabled = false;
        TakeHit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!runStarted || isGameOver)
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
        ui.SetLives(lives);

        if (lives == 0)
        {
            isGameOver = true;
            SaveBestScore();
            shieldVisual.SetActive(false);
            ui.SetPowerUps(0f, 0f, FishMultiplier);
            ui.SetGameOver(true);
            player.enabled = false;
            Time.timeScale = 0f;
        }
    }

    private void RefreshPowerUps()
    {
        float now = Time.time;
        float shieldSeconds = Mathf.Clamp(shieldEndTime - now, 0f, ShieldDuration);
        float multiplierSeconds = Mathf.Clamp(multiplierEndTime - now, 0f, FishDuration);
        bool showShield = shieldSeconds > 0f;
        if (shieldVisual.activeSelf != showShield)
            shieldVisual.SetActive(showShield);

        ui.SetPowerUps(shieldSeconds, multiplierSeconds, FishMultiplier);
    }

    private void SaveBestScore()
    {
        if (score <= bestScore)
            return;

        bestScore = score;
        PlayerPrefs.SetInt(BestScoreKey, bestScore);
        PlayerPrefs.Save();
        ui.SetBestScore(bestScore);
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
        replayRequested = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
