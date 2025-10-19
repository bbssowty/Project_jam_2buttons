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

    void OnEnable()
    {
        // When this level becomes active, snap to player position
        if (player != null)
            transform.position = player.position;
    }

    void Update()
    {
        if (completed) return;

        // Remove destroyed or missing targets
        targets.RemoveAll(t => t == null);

        // Check completion
        if (targets.Count == 0)
        {
            completed = true;
            Debug.Log($"{name} completed!");

            // If there's a next level, reactivate it
            if (nextLevel != null)
            {
                nextLevel.completed = false;          // mark next as active
                nextLevel.transform.position = player.position;
                nextLevel.gameObject.SetActive(true);
            }

            // Disable this level object
            gameObject.SetActive(false);
        }
    }
}
