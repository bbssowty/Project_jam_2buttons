using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public struct LevelData
    {
        public GameObject levelObject;       // The level GameObject
        public List<GameObject> targets;     // List of all targets in this level
    }

    [Header("Levels")]
    public List<LevelData> levels = new List<LevelData>();
    private int currentLevelIndex = 0;

    [Header("Player Reference")]
    public Transform player;

    [Header("Audio")]
    public AudioSource audioSource;          // Reference to AudioSource
    public AudioClip levelCompleteClip;      // Sound played when a level is completed
    [Range(0.5f, 1.5f)] public float pitchVariation = 0.1f;

    [Header("Events")]
    public UnityEvent onLevelCompleted;
    public UnityEvent onGameFinished;

    void Start()
    {
        // Ensure there’s an AudioSource
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        RestartLevels();
    }

    void Update()
    {
        if (currentLevelIndex >= levels.Count) return;

        LevelData current = levels[currentLevelIndex];

        // Remove destroyed or null targets
        current.targets.RemoveAll(t => t == null);

        // Check if all targets are inactive
        bool allTargetsInactive = true;
        foreach (var t in current.targets)
        {
            if (t != null && t.activeSelf)
            {
                allTargetsInactive = false;
                break;
            }
        }

        if (allTargetsInactive)
        {
            onLevelCompleted?.Invoke();
            PlayLevelCompleteSound();

            if (current.levelObject != null)
                current.levelObject.SetActive(false);

            currentLevelIndex++;

            if (currentLevelIndex < levels.Count)
            {
                LevelData next = levels[currentLevelIndex];
                if (next.levelObject != null)
                {
                    next.levelObject.SetActive(true);
                    if (player != null)
                        next.levelObject.transform.position = player.position;
                }
            }
            else
            {
                // Game finished
                onGameFinished?.Invoke();
            }
        }
    }

    public void RestartLevels()
    {
        foreach (var level in levels)
        {
            if (level.levelObject != null)
                level.levelObject.SetActive(false);

            // Reset all targets
            foreach (var t in level.targets)
            {
                if (t != null)
                    t.SetActive(true); // reset to active
            }
        }

        currentLevelIndex = 0;

        // Activate first level
        if (levels.Count > 0 && levels[0].levelObject != null)
        {
            levels[0].levelObject.SetActive(true);
            if (player != null)
                levels[0].levelObject.transform.position = player.position;
        }
    }

    private void PlayLevelCompleteSound()
    {
        if (audioSource != null && levelCompleteClip != null)
        {
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(levelCompleteClip);
        }
    }
}
