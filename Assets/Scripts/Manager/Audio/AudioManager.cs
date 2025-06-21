using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource audioSource;  // BGM
    private AudioSource sfxSource;    // SFX

    [Header("Clips")]
    public AudioClip bgmHome;
    public AudioClip bgmCombat;
    public AudioClip bgmLoading;
    public AudioClip sfxClick;
    public AudioClip sfxEatGem;
    public AudioClip sfxNormalAttack;
    public AudioClip sfxSwordSlash;
    public AudioClip sfxDealCards;
    public AudioClip sfxGoldGain;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // BGM
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // SFX
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip) return;

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void MuteMusic(bool mute)
    {
        if (audioSource != null)
            audioSource.mute = mute;
    }

    public bool IsMusicMuted()
    {
        return audioSource != null && audioSource.mute;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }
    public void StopSFXLoop()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
            sfxSource.loop = false;
        }
    }
    public void PlaySFXLoop(AudioClip clip)
    {
        if (sfxSource != null)
        {
            sfxSource.clip = clip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
    }


}
