using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    [SerializeField] private LevelUI levelUI;
    [SerializeField] private int testExpAmount = 100000;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"[TEST] Tăng {testExpAmount} EXP");
            levelUI.AddExp(testExpAmount);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            FindObjectOfType<KillFeedUI>().ShowKill("BotA", "BotB");
        }

    }
}