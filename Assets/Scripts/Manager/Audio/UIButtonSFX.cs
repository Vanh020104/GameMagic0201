using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClick);
    }
}
