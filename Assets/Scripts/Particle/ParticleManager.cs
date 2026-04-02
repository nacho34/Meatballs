using Unity.VisualScripting;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }
    public BlueSpawner BlueSpawner;
    public BrownSpawner BrownSpawner;
    public GreenSpawner GreenSpawner;
    // planne to be replaced with a dynamic system based on player position
    public Vector2 HeightBounds = new Vector2(-4f, 10f); 
    public Vector2 WidthBounds = new Vector2(-8f, 8f); // constant

    [SerializeField] private GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        HeightBounds.x = player.transform.position.y - 7f;
        HeightBounds.x = Mathf.Max(HeightBounds.x, -4f); // prevent going below initial bounds
        HeightBounds.y = player.transform.position.y + 7f;
        HeightBounds.y = Mathf.Max(HeightBounds.y, 10f); // prevent going below initial bounds
    }
}
