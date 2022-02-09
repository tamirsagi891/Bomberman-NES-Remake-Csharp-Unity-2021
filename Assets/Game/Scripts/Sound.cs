using UnityEngine;

// a sound class for the audio manager
[System.Serializable]
public class Sound
{
    [HideInInspector] public AudioSource audioSource;

    public string soundName;

    public AudioClip clip;

    public bool loop;

    [Range(0f, 1f)] public float volume;
}