using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyLoginRewardDatabase", menuName = "Database/DailyLoginRewardDatabase")]
public class DailyLoginRewardDatabase : ScriptableObject
{
    public List<DailyReward> rewards;
}