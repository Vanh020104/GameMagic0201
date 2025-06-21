using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/AttackTrick02")]
public class AttackTrick02 : BaseSkill
{
    public float range = 30f;
    public float angle = 90f;
    public int damage = 100;

    public override void Activate(PlayerInfo owner, Transform castPoint)
    {
        if (owner._mana < manaCost) return;
        owner._mana -= (int)manaCost;
        owner.GetComponent<Animator>()?.SetTrigger("Attack02");
          if (AudioManager.Instance != null && AudioManager.Instance.sfxSwordSlash != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxSwordSlash);
            }

        // DÙNG TÂM HERO để tính vùng chém
        Vector3 origin = owner.transform.position;

        Collider[] hits = Physics.OverlapSphere(origin, range);
        foreach (var hit in hits)
        {
            BotStats botTarget = hit.GetComponent<BotStats>();
            if (botTarget != null && botTarget.currentHP > 0)
            {
                Vector3 toTarget = botTarget.transform.position - origin;
                float distance = toTarget.magnitude;

                if (distance > range) continue;

                float angleToTarget = Vector3.Angle(owner.transform.forward, toTarget);

                if (angleToTarget <= angle / 2f)
                {
                    botTarget.currentHP -= damage;
                    if (owner != null && owner.isLocalPlayer)
                    {
                        DailyTaskBridge.Instance?.TryAddProgress("deal_500_damage", damage);
                        DailyTaskBridge.Instance?.TryAddProgress("deal_1000_damage", damage);
                    }

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

        // SINH HIỆU ỨNG TẠI ĐIỂM RIÊNG (castPoint) = skill01Prosition
        if (vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, castPoint.position, owner.transform.rotation);
            vfx.transform.localScale = Vector3.one * 5f;
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
