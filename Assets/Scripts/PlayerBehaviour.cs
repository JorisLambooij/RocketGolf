using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public Rigidbody playerRB;

    public bool invertX;
    public bool invertY;

    public float maxVelocity;

    public float launchForce;
    public float thrusterForce;
    public float steeringForce;
    public float passiveCorrection;

    public float fuel;
    public float fuel_consumption;

    private enum GamePhase { Launch, Fly };
    private GamePhase phase; 

    void Start()
    {
        phase = GamePhase.Launch;
    }

	void Update ()
    {
        switch (phase)
        {
            case GamePhase.Launch: Phase_Launch(); break;
            case GamePhase.Fly: Phase_Fly(); break;
        }
	}

    private void Phase_Launch()
    {
        playerRB.useGravity = false;
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 thrustForce = playerRB.transform.up * thrusterForce * launchForce * Time.deltaTime * 100;
            playerRB.AddForce(thrustForce);

            playerRB.useGravity = true;
            phase = GamePhase.Fly;
        }

        RotationControls();
    }

    private void Phase_Fly()
    {
        if (fuel > 0 && Input.GetKey(KeyCode.Space))
        {
            // Apply forward thrust
            Vector3 thrustForce = playerRB.transform.up * thrusterForce * Time.deltaTime * 100;
            playerRB.AddForce(thrustForce);
            fuel -= fuel_consumption * Time.deltaTime;
        }

        // Calculate the angle between the direction the rocket is facing
        // and the direction in which it is flying
        Vector3 currVelocity = playerRB.velocity;
        Vector3 currDirection = playerRB.transform.up * currVelocity.magnitude;
        float angle = Vector3.Angle(currDirection, currVelocity);

        // Apply the "correction force" only when the rocket is aligned properly ( -60° < angle < 60° )
        // This prevents the correction force from acting as a "brake force", slowing down the fall of the rocket
        if ( Mathf.Abs(angle) < 60 )
        {
            Vector3 correctionForce = (currDirection - currVelocity) * passiveCorrection * Time.deltaTime;
            playerRB.AddForce(correctionForce);
        }

        RotationControls();
    }

    // Method to handle the rotation of the rocket (WASD + Q,E Keys)
    void RotationControls()
    {
        // Torque for A and D keys
        Vector3 torque1 = -playerRB.transform.right * Input.GetAxis("Horizontal") * steeringForce;
        if (invertX)
            torque1 *= -1;
        // Torque for W and S keys
        Vector3 torque2 = -playerRB.transform.forward * Input.GetAxis("Vertical") * steeringForce;
        if (invertY)
            torque2 *= -1;
        // Torque for Q and E keys
        Vector3 torque3 = -playerRB.transform.up * Input.GetAxis("Roll") * steeringForce * 0.8f;

        // The sum of all inputs
        playerRB.AddTorque(torque1 + torque2 + torque3);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            phase = GamePhase.Launch;
            playerRB.useGravity = false;

            playerRB.transform.position += new Vector3(0, 4, 0);
            playerRB.velocity = Vector3.zero;
        }
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
