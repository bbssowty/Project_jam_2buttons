using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class MusicLayer
{
    public string name;
    public AudioSource source;
    [Range(0f, 1f)]
    public float volume = 1f;
}

public class MusicManager : MonoBehaviour
{
    [Header("Music Layers (order matters)")]
    public MusicLayer instruments;   // Base layer, always plays
    public MusicLayer drums;
    public MusicLayer bass;
    public MusicLayer melody;

    [Header("Intensity Settings")]
    [Range(0f, 1f)]
    public float intensity = 0f;        // Current music intensity
    public float drainRate = 0.1f;      // How fast intensity drains per second

    [Header("Fade Settings")]
    public float fadeDuration = 1f;     // Fade time for layers

    [Header("Events")]
    public UnityEvent onIncreaseIntensity;  // Call this to increase intensity externally

    [Header("Debug / Dummy Input")]
    public KeyCode testIncreaseKey = KeyCode.Space; // Press to increase intensity for testing
    public float testIncreaseAmount = 0.2f;

    private void Start()
    {
        // Initialize layers: play and loop
        InitializeLayer(instruments);
        InitializeLayer(drums);
        InitializeLayer(bass);
        InitializeLayer(melody);
    }

    private void Update()
    {
        // Drain intensity over time
        intensity -= drainRate * Time.deltaTime;
        intensity = Mathf.Clamp01(intensity);

        // Dummy input for testing
        if (Input.GetKeyDown(testIncreaseKey))
        {
            IncreaseIntensity(testIncreaseAmount);
            Debug.Log($"Test input: intensity increased to {intensity}");
        }

        // Update layers based on intensity
        UpdateLayers();
    }

    private void InitializeLayer(MusicLayer layer)
    {
        if (layer == null || layer.source == null) return;
        layer.source.loop = true;
        layer.source.volume = 0f;
        layer.source.Play();
    }

    private void UpdateLayers()
    {
        // Instruments always play
        SetLayerVolume(instruments, 1f);

        // Drums
        if (intensity > 0.25f)
            SetLayerVolume(drums, 1f);
        else
            SetLayerVolume(drums, 0f);

        // Bass
        if (intensity > 0.5f)
            SetLayerVolume(bass, 1f);
        else
            SetLayerVolume(bass, 0f);

        // Melody
        if (intensity > 0.75f)
            SetLayerVolume(melody, 1f);
        else
            SetLayerVolume(melody, 0f);
    }

    private void SetLayerVolume(MusicLayer layer, float targetVolume)
    {
        if (layer == null || layer.source == null) return;
        if (!layer.source.isPlaying)
            layer.source.Play();

        // Smooth volume change
        layer.source.volume = Mathf.Lerp(layer.source.volume, targetVolume, Time.deltaTime / fadeDuration);
    }

    /// <summary>
    /// Call this to increase intensity externally (e.g., from events)
    /// </summary>
    public void IncreaseIntensity(float amount)
    {
        intensity += amount;
        intensity = Mathf.Clamp01(intensity);
        onIncreaseIntensity?.Invoke();
    }
}
