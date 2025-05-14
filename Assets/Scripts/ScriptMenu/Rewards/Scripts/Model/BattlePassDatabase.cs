using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BattlePassDatabase", menuName = "BattlePass/BattlePassDatabase")]
public class BattlePassDatabase : ScriptableObject
{
    public List<BattlePassLevel> levels;
}
