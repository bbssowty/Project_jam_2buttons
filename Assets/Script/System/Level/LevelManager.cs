using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public struct LevelData
    {
        public GameObject levelObject;
        public List<GameObject> targets;
    }

    [Header("Levels")]
    public List<LevelData> levels = new List<LevelData>();
    private int currentLevelIndex = 0;

    [Header("Player Reference")]
    public Transform player;

    [Header("Events")]
    public UnityEvent onLevelCompleted;

    void Start()
    {
        RestartLevels();
    }

    void Update()
    {
        if (currentLevelIndex >= levels.Count) return;

        LevelData current = levels[currentLevelIndex];

        // Remove destroyed targets
        current.targets.RemoveAll(t => t == null);

        if (current.targets.Count == 0)
        {
            onLevelCompleted?.Invoke();

            current.levelObject.SetActive(false);

            currentLevelIndex++;
            if (currentLevelIndex < levels.Count)
            {
                LevelData next = levels[currentLevelIndex];
                next.levelObject.SetActive(true);
                if (player != null)
                    next.levelObject.transform.position = player.position;
            }
        }
    }

    public void RestartLevels()
    {
        // Reset all levels
        foreach (var level in levels)
        {
            if (level.levelObject != null)
                level.levelObject.SetActive(false);
        }

        // Reset index
        currentLevelIndex = 0;

        // Activate first level
        if (levels.Count > 0)
        {
            var first = levels[0];
            if (first.levelObject != null)
            {
                first.levelObject.SetActive(true);
                if (player != null)
                    first.levelObject.transform.position = player.position;
            }
        }
    }
}
