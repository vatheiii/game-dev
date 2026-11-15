using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private GameObject gameOverPanel;

    private const float HitCooldown = 1.5f;
    private int lives = 3;
    private float nextDamageTime;

    private void Start()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        UpdateLivesText();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider obstacle = collision.collider;
        if (lives == 0 || !obstacle.CompareTag("Obstacle"))
            return;

        // Let the cat pass the obstacle after contact.
        obstacle.enabled = false;

        if (Time.time < nextDamageTime)
            return;

        lives--;
        nextDamageTime = Time.time + HitCooldown;
        UpdateLivesText();

        if (lives == 0)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void UpdateLivesText()
    {
        livesText.text = $"Lives: {lives}";
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}