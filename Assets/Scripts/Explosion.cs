using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

    public float timer;
    public float explosionForce;
    public float explosionRadius;

    public bool isActive = false;

    public PlayerBehaviour fromPlayer;
    public List<Collider> colliders;


    // Use this for initialization
    void Start () {
        colliders = new List<Collider>();
	}
	
	// Update is called once per frame
	void Update () {
		if (timer > 0 && isActive)
        {
            timer -= Time.deltaTime;
            foreach(Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponentInParent<Rigidbody>();

                PlayerBehaviour playerScript = hit.GetComponentInParent<PlayerBehaviour>();
                if (rb != null && playerScript != null && playerScript != fromPlayer)
                {
                    if (!playerScript.activeShield)
                    {
                        rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                        playerScript.Grounded = true;
                    }
                    else
                        playerScript.CmdUpdateShield(false);
                }
            }
        }
        else
        {
            isActive = false;
            GameObject.Destroy(this.gameObject);
        }
	}

    void OnTriggerEnter(Collider coll)
    {
        colliders.Add(coll);
    }

    void OnTriggerLeave(Collider coll)
    {
        colliders.Remove(coll);
    }

}
