using UnityEngine;

public class OceanAudioManager : MonoBehaviour
{
    public static OceanAudioManager Instance;

    [Header("Source Audio")]
    public AudioClip waveClip;

    [Header("Grain Settings")]
    public float grainDuration = 0.2f;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    [Header("Intensity Mapping")]
    public float intensityDecay = 1.5f;
    public float minIntensityToSpawn = 50f;
    public float spawnRateMultiplier = 10f;
    public float maxSimultaneousGrains = 20;

    private float intensity;
    private float spawnTimer;

    void Awake()
    {
        Instance = this;
    }

    public void AddEnergy(float amount)
    {
        intensity += amount * 0.001f; // scale this!
    }

    void Update()
    {
        Debug.Log("Current Ocean Intensity: " + intensity); // Log the current intensity for debugging
        // decay intensity over time
        intensity = intensity * Mathf.Exp(-intensityDecay * Time.deltaTime);

        float spawnRate = intensity * spawnRateMultiplier;

        if (intensity < minIntensityToSpawn || transform.childCount >= maxSimultaneousGrains)
        {
            spawnTimer = 0f; // reset timer if we can't spawn
            return;
        }
        spawnTimer += Time.deltaTime * spawnRate;

        while (spawnTimer >= 1f)
        {
            spawnTimer -= 1f;
            SpawnGrain();
        }
    }

    void SpawnGrain()
    {
        if (waveClip == null) return;

        GameObject g = new GameObject("Grain");
        g.transform.parent = transform;

        AudioSource src = g.AddComponent<AudioSource>();
        src.clip = waveClip;

        // random start point
        float startTime = Random.Range(0f, waveClip.length - grainDuration);
        src.time = startTime;

        // random pitch
        src.pitch = Random.Range(minPitch, maxPitch);

        // volume based on intensity
        src.volume = Mathf.Clamp01(intensity);

        src.spatialBlend = 0f; // 2D sound (global ocean feel)

        src.Play();

        Destroy(g, grainDuration / src.pitch);
    }
}