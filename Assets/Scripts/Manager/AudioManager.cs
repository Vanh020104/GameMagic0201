using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource audioSource;

    [Header("Clips")]
    public AudioClip bgmHome;
    public AudioClip bgmCombat;
    public AudioClip bgmLoading;

    void Start()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("🔥 Reset PlayerPrefs: Đã xoá sạch dữ liệu, vào lại sẽ là tài khoản mới.");

    }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
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
        {
            audioSource.mute = mute;
        }
    }

    public bool IsMusicMuted()
    {
        if (audioSource != null)
        {
            return audioSource.mute;
        }
        return false;
    }

}
