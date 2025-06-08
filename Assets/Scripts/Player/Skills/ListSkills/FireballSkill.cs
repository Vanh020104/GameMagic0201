using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/FireballSkill")]
public class FireballSkill : BaseSkill
{
    public float range = 40f;
    public float angle = 90f;
    public int damage = 200;

    public override void Activate(PlayerInfo owner, Transform castPoint)
    {
        if (owner._mana < manaCost) return;
        owner._mana -= (int)manaCost;
        owner.GetComponent<Animator>()?.SetTrigger("Attack03");
        Vector3 origin = castPoint.position;
        Vector3 forward = owner.transform.forward;

        Collider[] hits = Physics.OverlapSphere(origin, range);
        foreach (var hit in hits)
        {
            BotStats botTarget = hit.GetComponent<BotStats>();
            if (botTarget != null && botTarget.currentHP > 0)
            {
                Vector3 toTarget = botTarget.transform.position - origin;
                float distance = toTarget.magnitude;

                if (distance > range) continue;

                float angleToTarget = Vector3.Angle(forward, toTarget);

                if (angleToTarget <= angle / 2f)
                {
                    botTarget.currentHP -= damage;  
                    // Hiển thị số damage
                    if (botTarget.floatingTextPrefab && botTarget.popupPoint)
                    {
                        var go = GameObject.Instantiate(botTarget.floatingTextPrefab, botTarget.popupPoint.position, Quaternion.identity);
                        var text = go.GetComponent<TextMeshPro>();
                        if (text != null)
                        {
                            text.text = damage.ToString();
                            text.color = Color.red; // đổi sang đỏ nếu là damage
                        }
                    }

                    if (botTarget.currentHP <= 0)
                    {
                        HandleKill(owner, botTarget);
                    }

                    Debug.Log($"Bot {botTarget.name} bị trúng! Distance: {distance}, Angle: {angleToTarget}");
                }

            }
        }

        if (vfxPrefab != null)
        {
            Quaternion rot = Quaternion.LookRotation(forward);
            rot *= Quaternion.Euler(0, -90, 0);
            GameObject vfx = Instantiate(vfxPrefab, castPoint.position, rot);
            vfx.transform.localScale = Vector3.one * 2.5f;
            Destroy(vfx, 0.8f);
        }

        void HandleKill(PlayerInfo owner, BotStats victim)
        {
            string killerName = PlayerPrefs.GetString("PlayerName", "Player");

            var controller = owner.GetComponent<PlayerController>();
            if (controller != null && controller.levelUI != null)
            {
                controller.levelUI.AddExp(30); // Hoặc 50 tùy bạn
            }

            if (owner.isLocalPlayer)
            {
                FindObjectOfType<KillInfoUIHandler>()?.AddKill();
            }

            FindObjectOfType<KillFeedUI>()?.ShowKill(killerName, victim.botName);
        }
    }
}
