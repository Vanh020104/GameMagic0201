using TMPro;
using UnityEngine;

public class KillFeedEntry : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void Setup(string killer, string victim)
    {
        text.text = $"{killer} killed {victim}";
        Destroy(gameObject, 2f); 
    }
}
