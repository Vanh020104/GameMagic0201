using UnityEngine;

[CreateAssetMenu(menuName = "Hero/HeroData")]
public class HeroData : ScriptableObject
{
    public string heroId;
    public string heroName;
    public Sprite heroIcon;
    public int baseHealth;
    public int baseDamage;
    public int baseSpeed;
    public int defaultLevel = 1;
    public int maxLevel = 50;    
    public string prefabPath;
    public SkillData[] skills = new SkillData[3];
}
