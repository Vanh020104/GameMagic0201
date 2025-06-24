using UnityEngine;

public class KillNotificationUI : MonoBehaviour
{
    public GameObject icon;
    public AudioClip killSfx; // 🎵 Âm thanh khi giết
    private AudioSource audioSource;

    void Start()
    {
        if (icon != null)
            icon.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Show()
    {
        if (icon != null)
        {
            icon.SetActive(true);
            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), 1.5f); // Hiện 1.5s rồi tắt
        }

        if (killSfx != null)
            audioSource.PlayOneShot(killSfx);
    }

    private void Hide()
    {
        if (icon != null)
            icon.SetActive(false);
    }
}
