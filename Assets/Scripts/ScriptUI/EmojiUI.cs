using UnityEngine;

public class EmojiUI : MonoBehaviour
{
    public GameObject emojiPanel;

    private void Start()
    {
        emojiPanel.SetActive(false);
    }

    public void ToggleEmojiPanel()
    {
        emojiPanel.SetActive(!emojiPanel.activeSelf);
    }

    public void OnEmojiSelected(string emoji)
    {
        Debug.Log($"[Emoji] Player chọn: {emoji}");

        emojiPanel.SetActive(false);
    }
}
