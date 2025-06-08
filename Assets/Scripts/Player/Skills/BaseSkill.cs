using UnityEngine;

public abstract class BaseSkill : ScriptableObject
{
    public string skillName;
    public float cooldown;
    public float manaCost;
    public GameObject vfxPrefab;

    public abstract void Activate(PlayerInfo owner, Transform castPoint);
}
