using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward : MonoBehaviour
{

    [SerializeField] private GameObject rewardPanel;
    // Start is called before the first frame update
    void Start()
    {
        rewardPanel.SetActive(false);
    }

    // Update is called once per frame
    public void CloseRewardPanel()
    {
        rewardPanel.SetActive(false);
    }

    public void OpenRewardPanel(){
        rewardPanel.SetActive(true);
    }
}
