using UnityEngine;
using UnityEngine.Events;

public class MenuManager : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onStartGame;

    [Header("References")]
    public GameObject player;
    public GameObject parentToDisable; // assign the parent object in inspector

    // Called by UI button
    public void StartGame()
    {
        // Hide the parent object
        if (parentToDisable != null)
            parentToDisable.SetActive(false);

        // Enable player
        if (player != null)
            player.SetActive(true);

        // Trigger event (for LevelManager to start first level)
        onStartGame?.Invoke();
    }

    // Called by UI button
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode
#else
        Application.Quit(); // Quit build
#endif
    }
}
