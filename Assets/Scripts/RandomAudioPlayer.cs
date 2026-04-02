using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class RandomAudioPlayer : MonoBehaviour
{
    [Header("Assign clips in Inspector")]
    public List<AudioClip> clips = new List<AudioClip>();

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays a random clip from the list
    /// </summary>
    public void PlayRandom()
    {
        if (clips == null || clips.Count == 0)
        {
            Debug.LogWarning("No audio clips assigned!");
            return;
        }

        int index = Random.Range(0, clips.Count);
        audioSource.clip = clips[index];
        audioSource.Play();
    }

    /// <summary>
    /// Plays a random clip without interrupting current audio
    /// (uses PlayOneShot instead)
    /// </summary>
    public void PlayRandomOneShot()
    {
        if (clips == null || clips.Count == 0)
        {
            Debug.LogWarning("No audio clips assigned!");
            return;
        }

        int index = Random.Range(0, clips.Count);
        audioSource.PlayOneShot(clips[index]);
    }
}