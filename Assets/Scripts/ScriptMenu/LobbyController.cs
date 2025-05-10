using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
   [SerializeField] private GameObject lobbyPanel;
    void Start()
    {
        lobbyPanel.SetActive(false);
    }

    public void OpenLobby(){
        lobbyPanel.SetActive(true);
    }
    public void CloseLobby(){
        lobbyPanel.SetActive(false);
    }
}
