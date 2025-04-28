using UnityEngine;
using UnityEngine.UI;

public class MusicToggleButton : MonoBehaviour
{
    public Sprite musicOnIcon;
    public Sprite musicOffIcon;

    private Button button;
    private Image buttonImage;
    private bool isMusicOn = true;

    private void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        button.onClick.AddListener(ToggleMusic);

        if (AudioManager.Instance != null && AudioManager.Instance.IsMusicMuted())
        {
            isMusicOn = false;
        }
        UpdateIcon();
    }

    private void ToggleMusic()
    {
        isMusicOn = !isMusicOn;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MuteMusic(!isMusicOn); 
        }

        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = isMusicOn ? musicOnIcon : musicOffIcon;
        }
    }
}
