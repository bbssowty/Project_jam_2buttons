using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timer = 0f;
    public bool timerRunning = false;

    [Header("UI")]
    public TextMeshProUGUI timerText; // TMP reference

    void Update()
    {
        if (!timerRunning) return;

        timer += Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = FormatTime(timer);
        }
    }

    public void StartTimer() => timerRunning = true;
    public void StopTimer() => timerRunning = false;
    public void ResetTimer()
    {
        timer = 0f;
        if (timerText != null)
            timerText.text = FormatTime(timer);
    }

    public string GetFormattedTime()
    {
        return FormatTime(timer);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return $"{minutes}:{seconds:00}:{milliseconds:00}";
    }
}
