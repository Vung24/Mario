using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource effectAudioSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip shootSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;

    }
    // Start is called before the first frame update
    void Start()
    {
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    public void PlayJumpSound()
    {
        if (effectAudioSource != null && jumpSound != null)
        {
            effectAudioSource.PlayOneShot(jumpSound);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
