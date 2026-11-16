using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private TMP_Text powerUpText;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button replayButton;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private Button playButton;
    [SerializeField] private Button startQuitButton;
    [SerializeField] private GameManager game;

    private int lastShieldTenths = -1;
    private int lastMultiplierTenths = -1;

    private void OnEnable()
    {
        if (game == null)
            return;

        playButton.onClick.AddListener(game.StartRun);
        startQuitButton.onClick.AddListener(game.QuitGame);
    }

    private void OnDisable()
    {
        if (game == null)
            return;

        playButton.onClick.RemoveListener(game.StartRun);
        startQuitButton.onClick.RemoveListener(game.QuitGame);
    }

    public void SetLives(int lives) => livesText.text = $"Lives: {lives}";
    public void SetScore(int score) => scoreText.text = $"Score: {score}";
    public void SetBestScore(int bestScore) => bestScoreText.text = $"Best: {bestScore}";
    public void SetGameOver(bool visible)
    {
        gameOverPanel.SetActive(visible);
        if (visible)
            replayButton.Select();
    }

    public void SetStartScreen(bool visible)
    {
        startScreen.SetActive(visible);
        hudCanvas.enabled = !visible;
        if (visible)
            playButton.Select();
        else if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetPowerUps(float shieldSeconds, float multiplierSeconds, int multiplier)
    {
        int shieldTenths = Mathf.CeilToInt(shieldSeconds * 10f);
        int multiplierTenths = Mathf.CeilToInt(multiplierSeconds * 10f);
        if (shieldTenths == lastShieldTenths && multiplierTenths == lastMultiplierTenths)
            return;

        lastShieldTenths = shieldTenths;
        lastMultiplierTenths = multiplierTenths;

        string shield = shieldTenths > 0 ? $"Shield {shieldTenths / 10f:0.0}s" : "";
        string scoreBoost = multiplierTenths > 0 ? $"Score x{multiplier} {multiplierTenths / 10f:0.0}s" : "";
        string separator = shieldTenths > 0 && multiplierTenths > 0 ? "  |  " : "";
        powerUpText.text = shield + separator + scoreBoost;
    }
}
