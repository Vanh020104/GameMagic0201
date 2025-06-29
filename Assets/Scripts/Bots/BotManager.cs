using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    public static BotManager Instance;

    public List<BotAI> allBots = new List<BotAI>();
    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterBot(BotAI bot)
    {
        if (!allBots.Contains(bot))
            allBots.Add(bot);
    }

    public void UnregisterBot(BotAI bot)
    {
        allBots.Remove(bot);
    }

    public void RegisterPlayer(PlayerInfo player)
    {
        if (!allPlayers.Contains(player))
            allPlayers.Add(player);
    }

    public void UnregisterPlayer(PlayerInfo player)
    {
        allPlayers.Remove(player);
    }
}
