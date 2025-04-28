using UnityEngine;

public class CurrencyTestInput : MonoBehaviour
{
    private GoldGemManager manager;

    private void Start()
    {
        manager = FindObjectOfType<GoldGemManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            manager.AddGold(100);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            manager.AddGem(50);
        }
    }
}
