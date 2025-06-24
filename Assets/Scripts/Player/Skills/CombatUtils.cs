using UnityEngine;

public static class CombatUtils
{
    public static void HandleKill(PlayerInfo owner, BotStats victim)
    {
        string killerName = PlayerPrefs.GetString("PlayerName", "Player");

        var controller = owner.GetComponent<PlayerController>();
        if (controller != null && controller.levelUI != null)
        {
            controller.levelUI.AddExp(70); // hoặc tùy chỉnh
        }

        if (owner.isLocalPlayer)
        {
            Object.FindObjectOfType<KillNotificationUI>()?.Show();
            Object.FindObjectOfType<KillInfoUIHandler>()?.AddKill();
        }

        Object.FindObjectOfType<KillFeedUI>()?.ShowKill(killerName, victim.botName);
    }
}
