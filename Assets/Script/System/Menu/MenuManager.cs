using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuParent;      // Main menu
    public GameObject pauseParent;     // Pause menu
    public GameObject endScreenParent; // End screen
    public TextMeshProUGUI endScreenTimerText; // Timer text on end screen

    [Header("Player Reference")]
    public GameObject player;

    [Header("Events")]
    public UnityEvent onStartGame;
    public UnityEvent onRestartGame;

    private bool isPaused = false;

    void Update()
    {
        // Only allow pause if not in main menu or end screen
        if ((menuParent != null && menuParent.activeSelf) ||
            (endScreenParent != null && endScreenParent.activeSelf))
            return;

        if (Input.GetKeyDown(KeyCode.Escape) && pauseParent != null)
        {
            TogglePause();
        }
    }

    public void StartGame()
    {
        if (menuParent != null)
            menuParent.SetActive(false);

        if (player != null)
        {
            player.SetActive(true);
            SetPlayerMovement(true);
        }

        onStartGame?.Invoke();
    }

    public void TogglePause()
    {
        if (pauseParent == null) return;

        isPaused = !isPaused;
        pauseParent.SetActive(isPaused);
        SetPlayerMovement(!isPaused);
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseParent != null)
            pauseParent.SetActive(false);

        SetPlayerMovement(true);
    }

    public void RestartGame()
    {
        ResumeGame();
        if (menuParent != null) menuParent.SetActive(false);
        if (pauseParent != null) pauseParent.SetActive(false);
        if (endScreenParent != null) endScreenParent.SetActive(false);

        SetPlayerMovement(true);
        onRestartGame?.Invoke();
    }

    public void ShowEndScreen(string finalTime)
    {
        if (endScreenParent != null)
            endScreenParent.SetActive(true);

        if (endScreenTimerText != null)
            endScreenTimerText.text = finalTime;

        SetPlayerMovement(false);
    }

    private void SetPlayerMovement(bool active)
    {
        if (player == null) return;

        var movement = player.GetComponent<PlayerController>();
        if (movement != null)
            movement.enabled = active;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = active;
    }
}
