using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RankDatabase", menuName = "Game/Rank Database")]
public class RankDatabase : ScriptableObject
{
    public RankInfo[] ranks;
}
