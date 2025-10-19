using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timer = 0f;                  // Current time elapsed
    public bool timerRunning = false;         // Is the timer active
    public UnityEvent<float> onTimerUpdate;   // Invoked every frame with current time

    [Header("UI References")]
    public TextMeshProUGUI timerText;         // Reference to the TMP text

    [Header("UI Update Settings")]
    [Tooltip("How often (in seconds) to update the timer text for smoother display.")]
    public float uiUpdateRate = 0.05f;        // Update every 50ms (20fps)

    private float nextUIUpdateTime = 0f;

    void Update()
    {
        if (!timerRunning) return;

        timer += Time.deltaTime;
        onTimerUpdate?.Invoke(timer);

        // Update the timer text only at a fixed rate
        if (Time.time >= nextUIUpdateTime)
        {
            nextUIUpdateTime = Time.time + uiUpdateRate;

            if (timerText != null)
                timerText.text = FormatTime(timer);
        }
    }

    public void StartTimer()
    {
        timerRunning = true;
        nextUIUpdateTime = 0f; // force immediate update on start
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResetTimer()
    {
        timer = 0f;
        onTimerUpdate?.Invoke(timer);

        if (timerText != null)
            timerText.text = FormatTime(timer);

        nextUIUpdateTime = 0f;
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return $"{minutes}:{seconds:00}:{milliseconds:00}";
    }
}
