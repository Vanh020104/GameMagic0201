using UnityEngine;

public static class GameFormula
{
    /// <summary>
    /// Trả về số EXP cần để lên cấp tiếp theo.
    /// </summary>
    public static int GetExpToNextLevel(int level)
    {
        return Mathf.RoundToInt(100 * Mathf.Pow(1.5f, level - 1));
    }
}
