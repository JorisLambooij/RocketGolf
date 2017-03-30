using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileManager : NetworkBehaviour {

    public GameObject BulletPrefab;

    public float bulletLifetime;

    private List<GameObject> bulletList;
    private List<float> bulletTimers;
    private List<GameObject> activeBulletList;
    

	// Use this for initialization
	void Start ()
    {
        bulletList = new List<GameObject>(100);
        activeBulletList = new List<GameObject>(100);
        bulletTimers = new List<float>(100);

        for (int i = 0; i < 1; i++)
        {
            GameObject bullet = Instantiate(BulletPrefab, this.transform);
            bullet.SetActive(false);
            bulletList.Add(bullet);
        }
    }
    
    // Receive incoming "shooting requests" and call the actual spawning method on all clients + host
    public void SpawnBullet(Vector3 position, Vector3 velocity)
    {
        Debug.Log(connectionToClient);
        RpcSpawnOnClient(position, velocity);
    }

    [ClientRpc]
    private void RpcSpawnOnClient(Vector3 position, Vector3 velocity)
    {
        GameObject b = Instantiate(BulletPrefab, this.transform);
        
        b.GetComponent<Rigidbody>().velocity = velocity;
        b.transform.position = position;
        b.transform.up = velocity.normalized;
        
        Destroy(b, 5);
    }
}
