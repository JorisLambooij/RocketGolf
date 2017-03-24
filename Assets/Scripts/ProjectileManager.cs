using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour {

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

        for (int i = 0; i < 50; i++)
        {
            GameObject bullet = Instantiate(BulletPrefab, this.transform);
            bullet.SetActive(false);
            bulletList.Add(bullet);
        }
    }
	
    public void SpawnBullet(Vector3 position, Vector3 velocity)
    {
        if(bulletList.Count > 0)
        {
            // "Free" Bullet available
            GameObject b = bulletList[bulletList.Count-1];

            b.SetActive(true);
            b.GetComponent<Rigidbody>().velocity = velocity;
            b.transform.position = position;
            b.transform.up = velocity.normalized;

            activeBulletList.Add(b);
            bulletTimers.Add(bulletLifetime);

            bulletList.RemoveAt(bulletList.Count-1);
        }
    }

	// Update is called once per frame
	void Update ()
    {
        for(int i = 0; i < activeBulletList.Count; i++)
        {
            bulletTimers[i] -= Time.deltaTime;
            if(bulletTimers[i] <= 0)
            {
                bulletTimers.RemoveAt(i);

                GameObject b = activeBulletList[i];
                b.SetActive(false);
                bulletList.Add(b);

                activeBulletList.RemoveAt(i);
                i--;
            }
        }
	}
}
