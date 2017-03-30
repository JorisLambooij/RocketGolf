using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameSettings : MonoBehaviour
{
    public int numberOfPlayers = 1;
    
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(GameObject.Find("BGM"));
    }

    public void ChangeNumberOfPlayers(Slider slider)
    {
        numberOfPlayers = (int) slider.value;
        NetworkManagerHUD netHUD = GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>();
        netHUD.offsetX = 70;
        netHUD.offsetY = 225;
        netHUD.showGUI = true;
        netHUD.offsetX = 70;
        netHUD.offsetY = 225;
    }
}
