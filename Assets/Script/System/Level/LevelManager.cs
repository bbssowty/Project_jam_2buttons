using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class LevelData
{
    public GameObject levelObject;         // The level GameObject
    public List<GameObject> targets;       // Exact targets that must be destroyed
}

public class LevelManager : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;               // Player reference

    [Header("Levels")]
    public List<LevelData> levels;         // All levels

    [Header("Events")]
    public UnityEvent onLevelCompleted;    // Triggered whenever a level is completed
    public UnityEvent onAllLevelsCompleted; // Triggered when the last level is completed

    private int currentLevelIndex = 0;

    void Start()
    {
        // Disable all levels first
        foreach (var level in levels)
            level.levelObject.SetActive(false);

        // Activate the first level
        ActivateLevel(0);
    }

    void Update()
    {
        if (currentLevelIndex >= levels.Count) return;

        LevelData currentLevel = levels[currentLevelIndex];

        // Remove destroyed or missing targets
        currentLevel.targets.RemoveAll(t => t == null);

        // Check completion
        if (currentLevel.targets.Count == 0)
        {
            Debug.Log($"{currentLevel.levelObject.name} completed!");
            currentLevel.levelObject.SetActive(false);

            // Trigger the Unity event for a single level
            onLevelCompleted?.Invoke();

            currentLevelIndex++;

            if (currentLevelIndex < levels.Count)
            {
                // Activate next level
                ActivateLevel(currentLevelIndex);
            }
            else
            {
                // Last level completed
                Debug.Log("All levels completed!");
                onAllLevelsCompleted?.Invoke();
            }
        }
    }

    private void ActivateLevel(int index)
    {
        if (index < 0 || index >= levels.Count) return;

        LevelData level = levels[index];
        level.levelObject.SetActive(true);

        // Snap level to player position
        if (player != null)
            level.levelObject.transform.position = player.position;
    }
}
