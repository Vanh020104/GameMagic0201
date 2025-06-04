using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingBattle : MonoBehaviour
{

    [SerializeField] private GameObject battleDetailPanel;
    
    void Start()
    {
        battleDetailPanel.SetActive(false);
    }

    public void OpenSetting() => battleDetailPanel.SetActive(true);
    public void ContinueBattle() => battleDetailPanel.SetActive(false);
}
