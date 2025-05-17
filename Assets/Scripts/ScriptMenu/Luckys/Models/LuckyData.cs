using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Lucky/Lucky Data")]
public class LuckyData : ScriptableObject
{
    public List<LuckyItemData> allRewards = new();
}
