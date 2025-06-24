using UnityEngine;

public class VictoryPopupController : MonoBehaviour
{
    public void OnClickContinue()
    {
        var endManager = FindObjectOfType<BattleEndManager>();
        if (endManager != null)
        {
            endManager.EndMatch();
        }
    }
}
