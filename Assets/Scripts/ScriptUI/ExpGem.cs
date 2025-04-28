using UnityEngine;

public class ExpGem : MonoBehaviour
{
    public int expAmount = 5;
    private ExpSpawner spawner;

    public void Init(ExpSpawner _spawner)
    {
        spawner = _spawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            if (player.levelUI != null)
            {
                player.levelUI.AddExp(expAmount);
                if (spawner != null)
                    spawner.RespawnAfterDelay(3f);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[Gem] Player.levelUI is NULL!");
            }
        }
    }
}
