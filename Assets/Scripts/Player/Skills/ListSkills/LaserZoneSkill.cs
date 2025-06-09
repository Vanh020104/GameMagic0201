using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/LaserZoneSkill_UsingPrefabButNoScriptOnIt")]
public class LaserZoneSkill : BaseSkill
{
    public new GameObject vfxPrefab;
    public float distanceInFront = 20f;
    public float aoeRadius = 5f;
    public float duration = 4.5f;
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
            vfxInstance.transform.localScale = Vector3.one * (aoeRadius / 2f); // scale VFX to match AOE
            GameObject.Destroy(vfxInstance, duration);
        }

        // Xử lý AOE
        owner.StartCoroutine(HandleAOE(center));
    }

    private IEnumerator HandleAOE(Vector3 center)
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
                    bot.SetSpeed(8f);

                    if (bot.botStats.currentHP <= 0)
                    {
                        bot.ForceDie();
                    }
                }
            }

            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }

        // Hết hiệu ứng thì trả lại speed gốc cho tất cả bot
        foreach (var bot in GameObject.FindObjectsByType<BotAI>(FindObjectsSortMode.None))
        {
            bot.ResetSpeed();
        }
    }
}
