using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    
    public Transform playerTransform;

    public float mouseSensitivity;
    public float cameraDistance;

    public bool invertX, invertY;
    //float distance, fov;
    float anglePitch = 0, angleYaw = 0; // Pitch = Up-Down; Yaw = Left/Right

    private int errorMessages;

    //private bool foundPlayerGO;

    void Start()
    {
        errorMessages = 0;
        /*
        foundPlayerGO = false;

        GameObject playerGO = GameObject.Find("PlayerRocket");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
            foundPlayerGO = true;
        }
        */
    }

    private bool SearchForPlayerGO()
    {
        GameObject playerGO = GameObject.Find("PlayerRocket(Clone)");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
            return true;
        }
        return false;
    }

	void Update ()
    {
        // Stop if player is not yet found
        if (playerTransform == null)
        {
            if (errorMessages < 5)
            {
                errorMessages++;
                return;
            }
            Debug.LogError("CameraBehaviourScript: PlayerTransform not found");
            return;
        }
        
        Vector2 mouseMovement = new Vector2 (Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        float invX = -1, invY = -1;

        if (invertX)
            invX = 1;
        if (invertY)
            invY = 1;

        // Fairly straight forward. Time.deltatime is commented out because i am not sure whether it is needed or not
        angleYaw   += invX * mouseMovement.x * (mouseSensitivity / 10); // * Time.deltaTime;
        anglePitch += invY * mouseMovement.y * (mouseSensitivity / 10); // * Time.deltaTime;

        // Avoid to big numbers
        if (angleYaw > 360)
            angleYaw -= 360;
        if (angleYaw < -360)
            angleYaw += 360;

        // Prevent the camera from tilting over/under the player (weird glitches otherwise)
        if (anglePitch > 90)
            anglePitch = 90;
        if (anglePitch < -45)
            anglePitch = -45;
        
        CalculatePosition();
    }

    // Calculate the camera offset to the player
    void CalculatePosition()
    {
        // Convert to radians
        float radPitch = Mathf.Deg2Rad * anglePitch;
        float radYaw = Mathf.Deg2Rad * angleYaw;

        // Horizontal distance (X and Z coordinates) is derived from the camera pitch
        // Higher pitch -> camera looks down -> horizontal distance minimal
        float horDistance = cameraDistance * Mathf.Cos(radPitch);
        float y = 4 + cameraDistance * Mathf.Sin(radPitch);

        // Calculate horizontal camera position
        float x = horDistance * Mathf.Cos(radYaw);
        float z = horDistance * Mathf.Sin(radYaw);
        
        // Apply the calulated offset
        Vector3 offset = new Vector3(x, y, z);
        this.transform.position = playerTransform.transform.position + offset;

        // Make the camera look at the rocket
        this.transform.rotation = Quaternion.Euler(Mathf.Sin(radPitch) * 90, -angleYaw - 90, 0);
    }

    public Vector3 CameraDirection
    {
        get { return (playerTransform.position - this.transform.position).normalized; }
    }
}
