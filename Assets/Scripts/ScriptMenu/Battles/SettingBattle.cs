using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingBattle : MonoBehaviour
{

    [SerializeField] private GameObject battleDetailPanel;
    [SerializeField] private GameObject settingPanel;

    void Start()
    {
        battleDetailPanel.SetActive(false);
        settingPanel.SetActive(false);
    }

    public void OpenSetting() => battleDetailPanel.SetActive(true);
    public void ContinueBattle() => battleDetailPanel.SetActive(false);
     public void OpenSettingPanel() => settingPanel.SetActive(true);

    /// <summary> Đóng setting </summary>
    public void CloseSettingPanel() => settingPanel.SetActive(false);
}
