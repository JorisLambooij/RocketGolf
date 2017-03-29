using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    public int numberOfPlayers = 1;
    
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void ChangeNumberOfPlayers(Slider slider)
    {
        numberOfPlayers = (int) slider.value;
    }
}
