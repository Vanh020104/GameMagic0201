using UnityEngine;

public class BotInfoSpawner : MonoBehaviour
{
    public GameObject worldUIPrefab;
    public Transform uiFollowPoint;

    void Start()
    {
        if (worldUIPrefab == null || uiFollowPoint == null) return;

        var stats = GetComponent<BotStats>();
        var ui = Instantiate(worldUIPrefab, uiFollowPoint.position, Quaternion.identity);
        ui.GetComponent<BotWorldUI>().Init(stats, uiFollowPoint);
        ui.transform.SetParent(null); 
        
        var ai = GetComponent<BotAI>();
        if (ai != null)
            ai.SetWorldUI(ui);
    }
}
