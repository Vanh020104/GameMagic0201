using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/ZoneSkill03")]
public class ZoneSkill03 : BaseSkill
{
    public float distanceInFront = 20f;
    public float aoeRadius = 5f;
    public float duration = 5f;
    public int damagePerTick = 100;
    public float tickRate = 0.5f;

    public override void Activate(PlayerInfo owner, Transform castPoint)
    {
        if (owner._mana < manaCost) return;

        owner._mana -= (int)manaCost;
        owner.GetComponent<Animator>()?.SetTrigger("Attack03");

        Vector3 center = castPoint.position + owner.transform.forward * distanceInFront;
        center.y = owner.transform.position.y;
        Quaternion rot = Quaternion.LookRotation(owner.transform.forward);

        // Hiện VFX
        GameObject vfxInstance = null;
        if (vfxPrefab != null)
        {
            vfxInstance = GameObject.Instantiate(vfxPrefab, center, rot);
            vfxInstance.transform.localScale = Vector3.one * (aoeRadius / 2f);
            GameObject.Destroy(vfxInstance, duration);
        }

        // Xử lý AOE
       owner.StartCoroutine(HandleAOE(center, owner));
    }

    private IEnumerator HandleAOE(Vector3 center, PlayerInfo owner)
    {
        float elapsed = 0f;
        HashSet<BotAI> botsHit = new HashSet<BotAI>();

        while (elapsed < duration)
        {
            Collider[] hits = Physics.OverlapSphere(center, aoeRadius);
            botsHit.Clear();

            foreach (var hit in hits)
            {
                BotAI bot = hit.GetComponent<BotAI>();
                if (bot != null && bot.botStats.currentHP > 0)
                {
                    botsHit.Add(bot);

                    bot.botStats.currentHP -= damagePerTick;
                    if (owner != null && owner.isLocalPlayer)
                    {
                        DailyTaskBridge.Instance?.TryAddProgress("deal_500_damage", damagePerTick);
                        DailyTaskBridge.Instance?.TryAddProgress("deal_1000_damage", damagePerTick);
                    }

                    // Hiển thị damage
                    if (bot.botStats.floatingTextPrefab && bot.botStats.popupPoint)
                    {
                        var go = GameObject.Instantiate(bot.botStats.floatingTextPrefab, bot.botStats.popupPoint.position, Quaternion.identity);
                        var text = go.GetComponent<TMPro.TextMeshPro>();
                        if (text != null)
                        {
                            text.text = damagePerTick.ToString();
                            text.color = Color.red;
                        }
                    }

                    bot.SetSpeed(5f);

                    if (bot.botStats.currentHP <= 0)
                    {
                        CombatUtils.HandleKill(owner, bot.botStats);
                        bot.ForceDie();
                    }
                }
            }

            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }

        // Reset speed
        foreach (var bot in GameObject.FindObjectsByType<BotAI>(FindObjectsSortMode.None))
        {
            bot.ResetSpeed();
        }
    }
}
