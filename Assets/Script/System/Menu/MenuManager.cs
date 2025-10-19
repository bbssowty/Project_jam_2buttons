using UnityEngine;
using UnityEngine.Events;

public class MenuManager : MonoBehaviour
{
    [Header("References")]
    public GameObject menuParent;      // Menu UI parent
    public GameObject pauseParent;     // Pause UI parent
    public GameObject player;          // Player object
    public UnityEvent onStartGame;     // Event to trigger first level
    public UnityEvent onRestartGame;   // Event to restart the game

    private bool isPaused = false;

    void Update()
    {
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
            player.SetActive(true);

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

        // NO timeScale modification; game continues running
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseParent != null)
            pauseParent.SetActive(false);
        // Game continues running normally
    }

    public void RestartGame()
    {
        ResumeGame();

        if (menuParent != null)
            menuParent.SetActive(false);
        if (pauseParent != null)
            pauseParent.SetActive(false);

        onRestartGame?.Invoke();
    }

    public void GoBack()
    {
        ResumeGame();
        if (menuParent != null)
            menuParent.SetActive(false);
        if (player != null)
            player.SetActive(true);
    }
}
