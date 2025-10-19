using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MusicLayer
{
    public string name;
    public AudioSource source;
    public float threshold; // activation threshold
    [HideInInspector] public float targetVolume = 0f;
}

public class MusicManager : MonoBehaviour
{
    [Header("Music Layers (in order)")]
    public MusicLayer instruments; // always active
    public MusicLayer drums;
    public MusicLayer bass;
    public MusicLayer melody;

    [Header("Intensity Settings")]
    public float intensity = 0f;
    public float drainRate = 0.1f;     // intensity drain per second

    [Header("Fade Settings")]
    public float fadeDuration = 1f;    // seconds to reach target volume
    public float maxLayerVolume = 0.5f; // volume for each layer when active

    [Header("Events")]
    public UnityEvent onIncreaseIntensity;

    [Header("Debug / Dummy Input")]
    public KeyCode testIncreaseKey = KeyCode.Space;
    public float testIncreaseAmount = 2f;

    private MusicLayer[] layers;

    private void Start()
    {
        layers = new MusicLayer[] { instruments, drums, bass, melody };

        foreach (var layer in layers)
        {
            if (layer.source == null) continue;
            layer.source.loop = true;
            layer.source.volume = 0f;
            layer.source.Play();
        }

        instruments.targetVolume = maxLayerVolume; // instruments always active
    }

    private void Update()
    {
        // Drain intensity
        intensity -= drainRate * Time.deltaTime;
        intensity = Mathf.Max(intensity, 0f);

        // Dummy input
        if (Input.GetKeyDown(testIncreaseKey))
        {
            IncreaseIntensity(testIncreaseAmount);
        }

        // Update each layer target volume
        UpdateLayerTargets();

        // Apply fade to actual volume
        foreach (var layer in layers)
        {
            if (layer.source == null) continue;
            layer.source.volume = Mathf.MoveTowards(layer.source.volume, layer.targetVolume, (maxLayerVolume / fadeDuration) * Time.deltaTime);
        }
    }

    private void UpdateLayerTargets()
    {
        foreach (var layer in layers)
        {
            if (layer == instruments) continue; // instruments always active

            if (intensity >= layer.threshold)
                layer.targetVolume = maxLayerVolume;
            else
                layer.targetVolume = 0f;
        }
    }

    public void IncreaseIntensity(float amount)
    {
        intensity += amount;
        onIncreaseIntensity?.Invoke();
    }
}
