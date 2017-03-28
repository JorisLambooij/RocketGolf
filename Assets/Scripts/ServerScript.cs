using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerScript : NetworkBehaviour {


    //public SyncList<GameObject> playerRockets;
    public int noOfPlayers;
    public SyncListBool playersReady;

    //[SyncVar]
    //public bool switchNow;

    [SyncVar]
    public bool isHost = false;

    public SyncListBool playersSwitched;

    [SyncVar]
    public PlayerBehaviour.GamePhase globalPhase;
    
    void Awake()
    {
        //playerRockets = new List<GameObject>();
        playersReady = new SyncListBool();
        playersSwitched = new SyncListBool();

        globalPhase = PlayerBehaviour.GamePhase.Prepare;
    }

    void Update()
    {
        if (!isHost || !isServer)
            return;

        if (playersReady.Count < noOfPlayers)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (!player.GetComponent<PlayerBehaviour>().registeredClient)
                {
                    player.GetComponent<PlayerBehaviour>().hostRocket = this.gameObject;
                    player.GetComponent<PlayerBehaviour>().pNo = RegisterRocket();
                    player.GetComponent<PlayerBehaviour>().registeredClient = true;
                }
            }
            return;
        }
        
        

        // Check if everyone is ready to continue
        bool allReady = true;
        foreach(bool ready in playersReady)
        {
            if (!ready)
                allReady = false;
        }
        // All clients are ready, so lets execute
        if (allReady)// && !switchNow)
        {
            //switchNow = true;

            switch (globalPhase)
            {
                case (PlayerBehaviour.GamePhase.Prepare): globalPhase = PlayerBehaviour.GamePhase.Launch; break;
                case (PlayerBehaviour.GamePhase.Launch): globalPhase = PlayerBehaviour.GamePhase.Fly; break;
                case (PlayerBehaviour.GamePhase.Fly): globalPhase = PlayerBehaviour.GamePhase.Wait; break;
                case (PlayerBehaviour.GamePhase.Wait): globalPhase = PlayerBehaviour.GamePhase.Prepare; break;
            }
        }
        /*
        if(switchNow)
        {
            bool allSwitched = true;
            foreach (bool switched in playersSwitched)
            {
                if (!switched)
                    allSwitched = false;
            }
            // All clients successfully switched phase, so clear everything
            if (allSwitched)
            {
                Debug.Log("All clients successfully switched phase, so clear everything");
                switchNow = false;
                for (int i = 0; i < playersSwitched.Count; i++)
                    playersSwitched[i] = false;
            }
        }*/
    }

    
    public int RegisterRocket()
    {
        playersReady.Add(false);
        playersSwitched.Add(false);
        //CmdRegisterRocket();
        return playersReady.Count;
    }
    [Command]
    private void CmdRegisterRocket()
    {
        playersReady.Add(false);
        playersSwitched.Add(false);
    }
}
