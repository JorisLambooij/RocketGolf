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

                if (rb != null && hit.GetComponentInParent<PlayerBehaviour>() != fromPlayer)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                    Debug.Log(hit.name);
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
