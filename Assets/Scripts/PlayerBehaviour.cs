using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public Rigidbody playerRB;
    public Transform goal;
    public ProjectileManager pManager;
    public CameraBehaviour cam;

    public bool invertX;
    public bool invertY;

    public float maxVelocity;

    public float launchForce;
    public float maxLaunchFuel;

    public float thrusterForce;
    public float steeringForce;
    public float passiveCorrection;

    public float fuel;
    public float fuel_consumption;

    public float maxHealth;

    public int ammo;
    public int magazineSize;
    public float ShotFrequency;
    public float DamagePerShot;
    public float reloadTime;

    private float health;

    private int magazine;
    private float reloadTimer;
    private bool reloading;
    private bool grounded;
    private float fuelTimer = 0;
    private bool fuelTimerIncreasing;
    private float launchTimer;
    private int countdown;
    private float fuelAfter;

    private float shotTimer;

    public enum GamePhase { Launch, Fly };
    private GamePhase phase; 

    void Start()
    {
        phase = GamePhase.Launch;
        fuelTimer = 0.2f;
        fuelTimerIncreasing = true;
        launchTimer = 5;
        countdown = 5;
        shotTimer = 0;
        magazine = magazineSize;
        ammo -= magazineSize;
        reloading = false;
        health = maxHealth;
    }

	void Update ()
    {
        switch (phase)
        {
            case GamePhase.Launch: Phase_Launch(); break;
            case GamePhase.Fly: Phase_Fly(); break;
        }

        if (grounded && playerRB.velocity.magnitude < 0.35)
        {
            phase = GamePhase.Launch;
            playerRB.useGravity = false;
            grounded = false;
            launchTimer = 5;

            playerRB.transform.position += new Vector3(0, 4, 0);
            playerRB.angularVelocity = Vector3.zero;

            Vector3 goalDir = goal.transform.position - playerRB.transform.position;

            float angle = Vector3.Angle(goalDir, Vector3.forward);
            if (goalDir.x < 0)
                angle *= -1;

            playerRB.transform.rotation = Quaternion.Euler(new Vector3(0,-90 + angle,-90));
        }
    }

    private void Phase_Launch()
    {
        Combat();
        LaunchCountdown();

        // The timer to swing the arrow back and forth
        // to determine the amount of fuel used for launch
        if (fuelTimerIncreasing)
            fuelTimer += Time.deltaTime;
        else
            fuelTimer -= Time.deltaTime;

        if (fuelTimer >= 1)
            fuelTimerIncreasing = false;
        else if (fuelTimer <= 0)
            fuelTimerIncreasing = true;
        
        // The fuel that will be left in the tank with the current timing
        fuelAfter = fuel - (fuelTimer) * maxLaunchFuel;
        
        // When there is less fuel left than the player could potentially use, adjust the timer mechanic
        if(fuel - maxLaunchFuel < 0)
        {
            // Timer goes between "0" and "fuel" instead of "fuel - maxLaunchFuel" and "fuel"
            // (In practice this means the arrow will swing slower)
            fuelAfter = (1 - fuelTimer) * fuel;
        }

        if (Input.GetKey(KeyCode.Space) || launchTimer <= 0)
        {
            // Adjust the force according to the amount of fuel consumed
            // Add a little bit to the fuel consumed to boost very weak launches (prevents "zero-force"-launches)
            Vector3 thrustForce = playerRB.transform.up * launchForce * (20 + fuel - fuelAfter) * Time.deltaTime * 20;
            playerRB.AddForce(thrustForce);
            
            // Adjust fuel stand
            fuel = fuelAfter;

            playerRB.useGravity = true;
            phase = GamePhase.Fly;

            countdown = 0;
        }
        playerRB.velocity = Vector3.zero;
        RotationControls();
    }

    private void LaunchCountdown()
    {
        countdown = (int)launchTimer + 1;
        // Maybe add super cool sound effects here?
        launchTimer -= Time.deltaTime;
    }

    private void Phase_Fly()
    {
        Combat();

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

        // Apply the "correction force" only when the rocket is aligned properly ( -30° < angle < 30° )
        // This prevents the correction force from acting as a "brake force", slowing down the fall of the rocket
        if ( Mathf.Abs(angle) < 30 && currVelocity.magnitude > maxVelocity * 0.4)
        {
            Vector3 correctionForce = (currDirection - currVelocity) * passiveCorrection * Time.deltaTime;
            playerRB.AddForce(correctionForce);
        }

        // Disable controls (except for thrust) after collision
        if (!grounded)
            RotationControls();
    }

    // Shooting etc.
    void Combat()
    {
        shotTimer -= Time.deltaTime;
        if (!reloading && shotTimer <= 0 && magazine > 0 && Input.GetMouseButton(0))
        {
            // Shoot
            //Debug.DrawRay(this.transform.position, cam.CameraDirection * 4, Color.red);
            //Debug.Break();
            
            pManager.SpawnBullet(transform.position + cam.CameraDirection * 4, cam.CameraDirection * 150);
            magazine--;
            shotTimer = 1 / ShotFrequency;
        }

        if (!reloading && ammo > 0 && (magazine == 0 || Input.GetKeyDown(KeyCode.R)))
        {
            // Reload
            reloadTimer = reloadTime;
            reloading = true;
        }
        if (reloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                reloading = false;
                int maxReload = Mathf.Min(ammo, magazineSize);
                int reloaded = Mathf.Min(maxReload, magazineSize - magazine);
                magazine += reloaded;
                ammo -= reloaded;
            }
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            health += DamagePerShot;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            health -= DamagePerShot;
        }
        health = Mathf.Clamp(health, 0, maxHealth);
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
            grounded = true;
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

    public float FuelTimer
    {
        get { return fuelTimer; }
    }

    public int Magazine
    {
        get { return magazine; }
    }

    public float CurrHealth
    {
        get { return health; }
    }

    public GamePhase CurrentPhase
    {
        get { return phase; }
    }

    public int Countdown
    {
        get { return countdown; }
    }

    public float FuelAfter
    {
        get { return fuelAfter; }
     }
}
