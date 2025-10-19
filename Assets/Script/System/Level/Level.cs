using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [Header("Assign all targets that must be destroyed")]
    public List<GameObject> targets = new List<GameObject>();

    [Header("Next level in sequence")]
    public Level nextLevel;

    [Header("Player reference")]
    public Transform player;

    [HideInInspector] public bool completed = false;

    // Internal copy of targets to avoid prefab sharing
    private List<GameObject> _runtimeTargets;

    void OnEnable()
    {
        // Make a runtime copy of the target list so each level is independent
        if (targets != null)
            _runtimeTargets = new List<GameObject>(targets);
        else
            _runtimeTargets = new List<GameObject>();

        // Snap level to player
        if (player != null)
            transform.position = player.position;

        completed = false;
    }

    void Update()
    {
        if (completed) return;

        // Remove destroyed or missing targets
        _runtimeTargets.RemoveAll(t => t == null);

        // Check completion
        if (_runtimeTargets.Count == 0)
        {
            completed = true;
            Debug.Log($"{name} completed!");

            // If there's a next level, reactivate it
            if (nextLevel != null)
            {
                nextLevel.gameObject.SetActive(true);        // activate next level
                if (nextLevel.player != null)
                    nextLevel.transform.position = player.position;
            }

            // Disable this level object
            gameObject.SetActive(false);
        }
    }
}
