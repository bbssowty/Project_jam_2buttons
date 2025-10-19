using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public GameObject levelObject;         // The level GameObject
    public List<GameObject> targets;       // Exact targets that must be destroyed
}

public class LevelManager : MonoBehaviour
{
    public Transform player;               // Player reference
    public List<LevelData> levels;         // All levels
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

            currentLevelIndex++;
            if (currentLevelIndex < levels.Count)
                ActivateLevel(currentLevelIndex);
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
