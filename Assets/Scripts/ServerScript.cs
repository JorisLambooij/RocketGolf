using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerScript : NetworkBehaviour {

    public List<GameObject> clientGOs;

	// Use this for initialization
	void Start ()
    {
        if (isServer && isLocalPlayer)
        {
            clientGOs = new List<GameObject>(8);
            Debug.Log("Im the server!");
            this.gameObject.name = "Player Host";
        }
        else
            this.enabled = false;
        //this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer || !isLocalPlayer)
            return;

        bool allReady = true;

        foreach (GameObject client in clientGOs)
        {
            if (!client.GetComponent<PlayerBehaviour>().ready)
                allReady = false;
        }
        if (allReady)
        {
            Debug.Log("All Ready!");
            foreach (GameObject client in clientGOs)
                client.GetComponent<PlayerBehaviour>().goLaunch = true;
            
            this.gameObject.GetComponent<PlayerBehaviour>().goLaunch = true;
        }
    }
}
