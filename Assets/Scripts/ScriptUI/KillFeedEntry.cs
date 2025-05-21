using TMPro;
using UnityEngine;

public class KillFeedEntry : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void Setup(string killer, string victim)
    {
        text.text = $"{killer} <color=red>killed</color> {victim}";
        Destroy(gameObject, 2f); 
    }
}
