using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public Rigidbody rigidbody;

    public float thrusterForce;
    public float steeringForce;

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
            // Always thrust forward ("up" from the rocket's POV)
            Vector3 thrusterDirection = rigidbody.transform.up;
            rigidbody.AddForce(thrusterDirection * thrusterForce);
            fuel -= fuel_consumption * Time.deltaTime;
        }

        // Torque for A and D keys
        Vector3 torque1 = rigidbody.transform.right * Input.GetAxis("Horizontal") * steeringForce;
        // Torque for W and S keys
        Vector3 torque2 = rigidbody.transform.forward * Input.GetAxis("Vertical") * steeringForce; 
        // The sum of both inputs
        rigidbody.AddTorque(torque1 + torque2);
    }

    void OnCollisionEnter(Collision collisionOBJ)
    {
        if (collisionOBJ.collider.CompareTag("Finish"))
        {
            Debug.Log("YOU WIN!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
}
