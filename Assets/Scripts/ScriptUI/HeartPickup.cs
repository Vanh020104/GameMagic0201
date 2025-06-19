using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    public int healAmount = 100;
    public float autoDestroyAfter = 30f;
    public bool isClaimed = false;
    void Start()
    {
        Destroy(gameObject, autoDestroyAfter); 
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerInfo player = other.GetComponentInParent<PlayerInfo>();
        if (player != null && player._hp < player._hpMax)
        {
            player._hp = Mathf.Min(player._hp + healAmount, player._hpMax);

            // Hiệu ứng hồi máu
            if (player.vfxHealPrefab && player.vfxAttachPoint)
            {
                GameObject vfx = Instantiate(player.vfxHealPrefab, player.vfxAttachPoint);
                vfx.transform.localPosition = Vector3.zero;
                Destroy(vfx, 3f);
            }

            GameManagerUI.currentHealItemCount--;
            Destroy(gameObject);
        }



        BotStats bot = other.GetComponentInParent<BotStats>();
        if (bot != null && bot.currentHP < bot.maxHP && !bot.isDead)
        {
            bot.currentHP = Mathf.Min(bot.currentHP + healAmount, bot.maxHP);
            bot.lastHealTime = Time.time;
            GameManagerUI.currentHealItemCount--;
            Destroy(gameObject);
        }
    }
}
