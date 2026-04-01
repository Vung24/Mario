using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioSource backGroundAudio;
    [SerializeField] private AudioSource effectAudio;
    [SerializeField] private AudioClip backgroundClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip shootClip;
    private bool SoundOn = true;

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
        if (backGroundAudio != null && backgroundClip != null)
        {
            backGroundAudio.clip = backgroundClip;
            backGroundAudio.loop = true;
            backGroundAudio.Play();
        }
    }
    public void PlayJumpSound()
    {
        if (effectAudio != null && jumpClip != null)
        {
            effectAudio.PlayOneShot(jumpClip);
        }
    }
    public void ToggleSound()
    {
        SoundOn = !SoundOn;
        if (backGroundAudio != null)
        {
            backGroundAudio.mute = !SoundOn;
        }

        if (effectAudio != null)
        {
            effectAudio.mute = !SoundOn;
        }
    }

    public bool IsSoundOn() => SoundOn;
    void Update()
    {
        
    }
}
