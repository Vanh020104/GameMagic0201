[System.Serializable]
public class HeroPlayerData
{
    public string heroId;
    public int currentLevel;

    public HeroPlayerData(string id, int level)
    {
        heroId = id;
        currentLevel = level;
    }
}