using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class SkillData
{
    public string skillName;
    public Sprite skillIcon;
    public LocalizedString description;
    public float cooldown;
}
