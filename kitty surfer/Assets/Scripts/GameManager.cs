using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject shieldVisual;

    private const float HitCooldown = 1f;
    private const float HitFlashDuration = 0.15f;
    private const float HitShieldFadeDuration = 0.2f;
    private const float UnitsPerPoint = 10f;
    private const string BestScoreKey = "BestScore";
    private const float FishDuration = 5f;
    private const int FishMultiplier = 2;
    private const float ShieldDuration = 5f;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly Color HitFlashColor = new Color(1f, 0.34f, 0.27f);
    private static bool replayRequested;
    private int lives = 3;
    private float damageBlockedUntil;
    private float startZ;
    private int score;
    private int bestScore;
    private bool runStarted;
    private bool isGameOver;
    private int distanceSteps;
    private float multiplierEndTime;
    private float shieldEndTime;
    private SkinnedMeshRenderer kittyRenderer;
    private Renderer shieldRenderer;
    private Color shieldColor;
    private MaterialPropertyBlock effectColors;
    private bool hitFlashActive;

    private bool IsMultiplierActive => Time.time < multiplierEndTime;
    private bool IsShieldActive => Time.time < shieldEndTime;

    private void Start()
    {
        kittyRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        shieldRenderer = shieldVisual.GetComponent<Renderer>();
        shieldColor = shieldRenderer.sharedMaterial.GetColor(BaseColor);
        effectColors = new MaterialPropertyBlock();
        ui.SetGameOver(false);
        startZ = transform.position.z;
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        ui.SetBestScore(bestScore);
        ui.SetLives(lives);
        ui.SetScore(score);
        RefreshEffects();

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

        RefreshEffects();
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
        if (Time.time < damageBlockedUntil)
            return;

        if (IsShieldActive)
        {
            shieldEndTime = 0f;
            RefreshEffects();
            return;
        }

        lives--;
        damageBlockedUntil = Time.time + HitCooldown;
        ui.SetLives(lives);

        isGameOver = lives == 0;
        RefreshEffects();
        if (!isGameOver)
            return;

        SaveBestScore();
        ui.SetGameOver(true);
        player.enabled = false;
        Time.timeScale = 0f;
    }

    private void RefreshEffects()
    {
        float now = Time.time;
        float recoverySeconds = isGameOver ? 0f : damageBlockedUntil - now;
        float shieldSeconds = isGameOver ? 0f : Mathf.Clamp(shieldEndTime - now, 0f, ShieldDuration);
        float multiplierSeconds = isGameOver ? 0f : Mathf.Clamp(multiplierEndTime - now, 0f, FishDuration);

        UpdateHitVisuals(recoverySeconds, shieldSeconds > 0f);
        ui.SetPowerUps(shieldSeconds, multiplierSeconds, FishMultiplier);
    }

    private void UpdateHitVisuals(float recoverySeconds, bool shieldPickupActive)
    {
        bool flash = recoverySeconds > HitCooldown - HitFlashDuration;
        if (flash != hitFlashActive)
        {
            effectColors.SetColor(BaseColor, HitFlashColor);
            kittyRenderer.SetPropertyBlock(flash ? effectColors : null);
            hitFlashActive = flash;
        }

        bool showBubble = shieldPickupActive || recoverySeconds > 0f;
        if (shieldVisual.activeSelf != showBubble)
            shieldVisual.SetActive(showBubble);

        if (!showBubble)
            return;

        Color color = shieldColor;
        if (!shieldPickupActive)
            color.a *= Mathf.Clamp01(recoverySeconds / HitShieldFadeDuration);

        effectColors.SetColor(BaseColor, color);
        shieldRenderer.SetPropertyBlock(effectColors);
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
