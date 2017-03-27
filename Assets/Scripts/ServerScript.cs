using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerScript : NetworkBehaviour {


    //public SyncList<GameObject> playerRockets;
    public int noOfPlayers;
    public SyncListBool playersReady;

    [SyncVar]
    public bool switchNow;

    [SyncVar]
    public bool isHost = false;

    public SyncListBool playersSwitched;

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
        if (!isHost)
            return;

        if (!isServer || playersReady.Count < noOfPlayers)
            return;
        
        // Check if everyone is ready to continue
        bool allReady = true;
        foreach(bool ready in playersReady)
        {
            if (!ready)
                allReady = false;
        }
        // All clients are ready, so lets execute
        if (allReady)
            switchNow = true;

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
                switchNow = false;
                for (int i = 0; i < playersSwitched.Count; i++)
                    playersSwitched[i] = false;
            }
        }
    }

    
    public int RegisterRocket()
    {
        CmdRegisterRocket();
        return playersReady.Count + 1;
    }
    [Command]
    private void CmdRegisterRocket()
    {
        playersReady.Add(false);
        playersSwitched.Add(false);
    }
}
