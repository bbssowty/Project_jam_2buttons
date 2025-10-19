using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class LevelEntry
{
    public GameObject levelObject;        // Parent GameObject for this level
    public List<GameObject> targets;      // List of all targets in this level
}

public class LevelManager : MonoBehaviour
{
    [Header("Levels in order")]
    public List<LevelEntry> levels = new List<LevelEntry>();

    [Header("Events")]
    public UnityEvent onLevelCompleted;

    private int currentLevelIndex = -1;

    [Header("References")]
    public Transform player;


    void Update()
    {
        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Count) return;

        LevelEntry current = levels[currentLevelIndex];
        if (current.targets == null || current.targets.Count == 0) return;

        // Remove destroyed targets
        current.targets.RemoveAll(t => t == null);

        // Check if level is complete
        if (current.targets.Count == 0)
        {
            CompleteCurrentLevel();
        }
    }

    public void StartFirstLevel()
    {
        if (levels.Count == 0) return;

        currentLevelIndex = 0;
        ActivateLevel(currentLevelIndex);
    }

    private void ActivateLevel(int index)
    {
        if (index < 0 || index >= levels.Count) return;

        LevelEntry entry = levels[index];

        if (entry.levelObject != null)
        {
            // Move level to player's position
            if (player != null)
                entry.levelObject.transform.position = player.position;

            // Activate level
            entry.levelObject.SetActive(true);
        }

        // Ensure all targets are active
        foreach (var t in entry.targets)
        {
            if (t != null)
                t.SetActive(true);
        }

        Debug.Log($"Level {index + 1} activated: {entry.levelObject.name}");
    }


    public void CompleteCurrentLevel()
    {
        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Count) return;

        LevelEntry current = levels[currentLevelIndex];

        // Deactivate current level
        if (current.levelObject != null)
            current.levelObject.SetActive(false);

        // Trigger event
        onLevelCompleted?.Invoke();

        // Move to next level
        currentLevelIndex++;
        if (currentLevelIndex < levels.Count)
            ActivateLevel(currentLevelIndex);
        else
            Debug.Log("All levels completed!");
    }
}
