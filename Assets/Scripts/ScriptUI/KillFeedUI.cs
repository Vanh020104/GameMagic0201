using UnityEngine;

public class KillFeedUI : MonoBehaviour
{
    [SerializeField] private Transform feedPanel;
    [SerializeField] private GameObject killTextPrefab;

    public void ShowKill(string killer, string victim)
    {
        GameObject go = Instantiate(killTextPrefab, feedPanel);
        go.GetComponent<KillFeedEntry>().Setup(killer, victim);
    }
}
