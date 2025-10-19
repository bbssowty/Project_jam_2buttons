using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("References")]
    public GameObject menuParent;       // Main menu UI parent
    public GameObject pauseParent;      // Pause menu UI parent
    public GameObject endScreenParent;  // End screen UI parent
    public TextMeshProUGUI finalTimeText; // TMP text to display final time
    public GameObject player;           // Player GameObject

    [Header("Events")]
    public UnityEvent onStartGame;      // Event to trigger first level
    public UnityEvent onRestartGame;    // Event to restart the game

    private bool isPaused = false;

    void Update()
    {
        // Only allow pausing if main menu is NOT active (game in progress)
        if (menuParent != null && menuParent.activeSelf)
            return;

        // Prevent pausing when end screen is active
        if (endScreenParent != null && endScreenParent.activeSelf)
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

        if (endScreenParent != null)
            endScreenParent.SetActive(false);

        if (player != null)
        {
            player.SetActive(true);
            SetPlayerMovement(true);
        }

        onStartGame?.Invoke();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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

        if (menuParent != null)
            menuParent.SetActive(false);
        if (pauseParent != null)
            pauseParent.SetActive(false);
        if (endScreenParent != null)
            endScreenParent.SetActive(false);

        if (player != null)
            SetPlayerMovement(true);

        onRestartGame?.Invoke();
    }

    public void GoBack()
    {
        ResumeGame();

        if (menuParent != null)
            menuParent.SetActive(false);

        if (player != null)
        {
            player.SetActive(true);
            SetPlayerMovement(true);
        }
    }

    /// <summary>
    /// Called when the game ends to display the final timer.
    /// </summary>
    public void ShowEndScreen(string finalTime)
    {
        // Disable all menus
        if (menuParent != null) menuParent.SetActive(false);
        if (pauseParent != null) pauseParent.SetActive(false);

        // Show the end screen
        if (endScreenParent != null)
            endScreenParent.SetActive(true);

        // Display the final timer
        if (finalTimeText != null)
            finalTimeText.text = finalTime;

        // Disable player controls
        SetPlayerMovement(false);
    }

    private void SetPlayerMovement(bool active)
    {
        if (player == null) return;

        // Replace "PlayerController" with your player movement script’s name
        var movement = player.GetComponent<PlayerController>();
        if (movement != null)
            movement.enabled = active;

        // Optional: control physics simulation
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = active;
    }
}
