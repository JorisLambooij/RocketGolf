using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public Rigidbody playerRB;

    public float maxVelocity;

    public float thrusterForce;
    public float steeringForce;
    public float passiveCorrection;

    public float fuel;
    public float fuel_consumption;

	void Update ()
    {
        // For now, just test the flying-phase
        Phase_Fly();
	}

    private void Phase_Launch()
    {
        // Still needs to be implemented properly
    }

    private void Phase_Fly()
    {
        if (fuel > 0 && Input.GetKey(KeyCode.Space))
        {
            Vector3 thrustForce = playerRB.transform.up * thrusterForce;
            playerRB.AddForce(thrustForce);
        }

        Vector3 currVelocity = playerRB.velocity;
        Vector3 currDirection = playerRB.transform.up * currVelocity.magnitude;
        Vector3 correctionForce = (currDirection - currVelocity) * passiveCorrection * Time.deltaTime;

        //Vector3 force = playerRB.transform.up * passiveThrust;
        //vel += new Vector3(0, -1, 0) * Gravity.gravityForce * Time.deltaTime;

        playerRB.AddForce(correctionForce);
        playerRB.AddForce(new Vector3(0, -1, 0) * Gravity.gravityForce * Time.deltaTime);
        
        //Debug.Log("current v: " + currVelocity.magnitude);

        // Torque for A and D keys
        Vector3 torque1 = playerRB.transform.right * Input.GetAxis("Horizontal") * steeringForce;
        // Torque for W and S keys
        Vector3 torque2 = playerRB.transform.forward * Input.GetAxis("Vertical") * steeringForce; 
        // The sum of both inputs
        playerRB.AddTorque(torque1 + torque2);
    }

    void OnTriggerEnter(Collider collider)
    {
        // Check for Power-Ups here
        if (collider.CompareTag("Finish"))
        {
            Debug.Log("YOU WIN!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
}
