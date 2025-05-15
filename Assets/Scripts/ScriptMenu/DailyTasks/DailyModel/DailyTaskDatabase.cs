using UnityEngine;

[CreateAssetMenu(menuName = "DailyTasks/DailyTaskDatabase")]
public class DailyTaskDatabase : ScriptableObject
{
    public DailyTaskData[] tasks;
}
