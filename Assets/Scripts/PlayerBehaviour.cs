using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerBehaviour : NetworkBehaviour
{
    [SyncVar (hook= "changePNo")]
    public int pNo;

    private void changePNo(int newPno)
    {
        pNo = newPno;
        playerRB.position = GameObject.Find("Launch Pad P" + newPno).transform.position + new Vector3(0, 4, 0);
        playerRB.transform.rotation = Quaternion.Euler(new Vector3(0, -90, -60));
    }

    [SyncVar]
    public bool registeredClient;
    [SyncVar]
    public bool ready = false;
    [SyncVar]
    public GameObject hostRocket;
    //[SyncVar]
    //public string GOname = "PlayerRocket";

    public Rigidbody playerRB;
    public Transform goal;
    public ProjectileManager pManager;
    public CameraBehaviour cam;
    public GameObject shield;
    public GameObject explosionprefab;

    public bool invertX;
    public bool invertY;

    #region Rocket's Stats
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
    #endregion

    private float health;

    private float prepareTimer;

    private int magazine;
    private float reloadTimer;
    private bool reloading;
    private bool grounded;
    
    private float shieldTimer;

    private bool launchPressed;
    private float fuelTimer = 0;
    private bool fuelTimerIncreasing;
    private float launchTimer;
    private int countdown;
    private float fuelAfter;

    private float shotTimer;
    
    private PowerUp.Type itemSlot;

    public PlayerBehaviour hostScript;
    
    public enum GamePhase { Wait, Prepare, Launch, Fly };
    private GamePhase phase;

    void Start()
    {
        //this.gameObject.name = this.GOname;
        phase = GamePhase.Wait;
        phase = GamePhase.Prepare;

        fuelTimer = 0.2f;
        fuelTimerIncreasing = true;
        prepareTimer = 30;
        launchTimer = 5;
        countdown = 5;
        shotTimer = 0;
        magazine = magazineSize;
        ammo -= magazineSize;
        reloading = false;
        health = maxHealth;
        //goLaunch = false;
        ready = false;
        itemSlot = PowerUp.Type.NULL;

        goal = GameObject.Find("Goal").transform;
        pManager = GameObject.Find("Projectile Manager GO").GetComponent<ProjectileManager>();
        cam = GameObject.Find("Camera").GetComponent<CameraBehaviour>();

        registeredClient = false;
        if (isLocalPlayer)
        {
            cam.playerTransform = this.transform;
            this.gameObject.name = "PlayerRocket (Local)";

            if (!isServer)
                return;
            hostRocket = this.gameObject;

            // Register this rocket (The host must know himself too)
            pNo = hostRocket.GetComponent<ServerScript>().RegisterRocket();
            hostRocket.GetComponent<ServerScript>().isHost = true;

            if (pNo != 0)
                registeredClient = true;

            playerRB.position = GameObject.Find("Launch Pad P" + pNo).transform.position + new Vector3(0, 4, 0);
            playerRB.transform.rotation = Quaternion.Euler(new Vector3(0, -90, -60));
        }
    }
    
	void Update ()
    {
        currPhase = phase;

        // process only the local player, ignore other players
        if (!isLocalPlayer || !registeredClient)
            return;
        
        if( (phase == GamePhase.Wait && hostRocket.GetComponent<ServerScript>().globalPhase == GamePhase.Prepare) || phase != hostRocket.GetComponent<ServerScript>().globalPhase)
            SwitchPhase();

        switch (phase)
        {
            case GamePhase.Prepare: Phase_Prepare(); break;
            case GamePhase.Launch: Phase_Launch(); break;
            case GamePhase.Fly: Phase_Fly(); break;
            case GamePhase.Wait: Phase_Wait(); break;
        }
    }

    private void Phase_Wait()
    {
        playerRB.velocity = Vector3.zero;
        playerRB.angularVelocity = Vector3.zero;

        PutReady();
    }

    public void SwitchPhase()
    {
        // This object will now switch phase, so tell the Network Manager
        PutNotReady();
        hostRocket.GetComponent<ServerScript>().playersSwitched[pNo - 1] = true;

        switch (phase)
        {
            case GamePhase.Wait:
                SwitchToPreparePhase();
                playerRB.angularDrag = 3;
                break;
            case GamePhase.Fly:
                phase = GamePhase.Wait;
                break;
            case GamePhase.Prepare:
                phase = GamePhase.Launch;
                playerRB.angularDrag = 4;
                launchTimer = 5;
                break;
            case GamePhase.Launch:
                phase = GamePhase.Fly;
                launchPressed = false;
                break;
        }
    }

    private void Phase_Prepare()
    {
        playerRB.velocity = Vector3.zero;

        if(!ready)
            RotationControls();
        
        // Press Joystick.A, Spacebar or wait for the timer
        if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Space) || prepareTimer <= 0)
        {
            PutReady();
        }
    }

    private void Phase_Launch()
    {
        Combat();
        LaunchCountdown();

        if(!ready)
            playerRB.velocity = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Space))
        {
            launchPressed = true;
        }
        
        if (!launchPressed)
        {
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
            if (fuel - maxLaunchFuel < 0)
            {
                // Timer goes between "0" and "fuel" instead of "fuel - maxLaunchFuel" and "fuel"
                // (In practice this means the arrow will swing slower)
                fuelAfter = (1 - fuelTimer) * fuel;
            }
        }

        if (launchTimer <= 0 && !ready)
        {
            // Adjust the force according to the amount of fuel consumed
            // Add a little bit to the fuel consumed to boost very weak launches (prevents "zero-force"-launches)
            Vector3 thrustForce = playerRB.transform.up * launchForce * (20 + fuel - fuelAfter) * Time.deltaTime * 20;
            playerRB.AddForce(thrustForce);

            // Adjust fuel stand
            
            fuel = fuelAfter;
            playerRB.useGravity = true;
            PutReady();
            countdown = 0;
        }
    }

    private void PrepareCountdown()
    {
        prepareTimer -= Time.deltaTime;
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

        if (fuel > 0 && (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Space)))
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
        // After collision, wait until the rocket stops moving, then switch to Wait Phase
        else if(playerRB.velocity.magnitude < 0.35f)
        {
            PutReady();
            //SwitchPhase();
        }
    }

    private void SwitchToPreparePhase()
    {
        phase = GamePhase.Prepare;
        playerRB.useGravity = false;
        grounded = false;
        launchTimer = 5;
        prepareTimer = 30;

        CmdPutNotReady();

        fuel = Mathf.Min(100, fuel + 40);
        fuelAfter = fuel;

        health = Mathf.Min(maxHealth, health + maxHealth / 2);

        ammo += 20;

        playerRB.drag = 0;

        playerRB.transform.position += new Vector3(0, 4, 0);
        playerRB.angularVelocity = Vector3.zero;
        playerRB.velocity = Vector3.zero;

        Vector3 goalDir = goal.transform.position - playerRB.transform.position;

        float goalAngle = Vector3.Angle(goalDir, Vector3.forward);
        if (goalDir.x < 0)
            goalAngle *= -1;

        playerRB.transform.rotation = Quaternion.Euler(new Vector3(0, -90 + goalAngle, -60));
    }

    // Shooting etc.
    void Combat()
    {
        shotTimer -= Time.deltaTime;
        if (!reloading && shotTimer <= 0 && magazine > 0 && Input.GetAxis("P1Fire") > 0)
        {
            pManager.SpawnBullet(transform.position + cam.CameraDirection * 4, cam.CameraDirection * 150);
            magazine--;
            shotTimer = 1 / ShotFrequency / Input.GetAxis("P1Fire");
        }

        // Reload when the magazine is empty or when the button is pressed.
        // But only if:
        // - We are not already reloading
        // - We have ammo left
        // - Our Magazine is not full already
        if ((magazine == 0 || Input.GetAxis("P1Reload") != 0) && !reloading && ammo > 0 && magazine != magazineSize)
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

        Bomb();
        Shield();
    }

    // Method to handle the rotation of the rocket (WASD + Q,E Keys)
    void RotationControls()
    {
        // Torque for A and D keys
        Vector3 torque1 = -playerRB.transform.right * Input.GetAxis("P1Horizontal") * steeringForce;
        if (invertX)
            torque1 *= -1;
        // Torque for W and S keys
        Vector3 torque2 = -playerRB.transform.forward * Input.GetAxis("P1Vertical") * steeringForce;
        if (invertY)
            torque2 *= -1;
        // Torque for Q and E keys
        Vector3 torque3 = -playerRB.transform.up * Input.GetAxis("P1Roll") * steeringForce * 0.8f;

        // The sum of all inputs
        playerRB.AddTorque(torque1 + torque2 + torque3);
    }

    void I_Win()
    {
        Cmd_I_Win();
    }

    [Command]
    private void Cmd_I_Win()
    {
        hostRocket.GetComponent<ServerScript>().winningPlayer = this.pNo;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            grounded = true;
            playerRB.drag = 0.0005f;
            playerRB.angularDrag = 8;
        }
    }
    
    void OnTriggerEnter(Collider collider)
    {
        // Check for Power-Ups here
        if (collider.CompareTag("Finish"))
        {
            I_Win();
        }

        if (collider.CompareTag("Power-Up"))
        {
            // Remove power up object
            Destroy(collider.gameObject);
            PowerUp powerUp = collider.GetComponent<PowerUp>();

            switch (powerUp.type)
            {
                case (PowerUp.Type.Fuel): 
                    // 15% fuel per Power-Up lvl
                    fuel += powerUp.value * 15;
                    fuel = Mathf.Min(fuel, 100);
                    break;
                case (PowerUp.Type.Speed):
                    // ThrusterForce equal to the force the player would get when thrusting for 1 sec (roughly) 
                    // Multiplied with Power-Up lvl
                    Vector3 thrustForce = playerRB.transform.up * thrusterForce * 60 * powerUp.value;
                    playerRB.AddForce(thrustForce);
                    break;
                case (PowerUp.Type.Health):
                    // 15% health regained per Power-Up lvl
                    health += maxHealth * 15 * powerUp.value;
                    break;
                case (PowerUp.Type.Bomb):
                    // Player now has a Bomb
                    itemSlot = PowerUp.Type.Bomb;
                    break;
                case (PowerUp.Type.Shield):
                    // Player now has a Shield
                    itemSlot = PowerUp.Type.Shield;
                    break;
            }
        }
    }

    #region Use-Items
    void Bomb()
    {
        if (itemSlot == PowerUp.Type.Bomb && (Input.GetKeyDown(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.F)))
        {
            itemSlot = PowerUp.Type.NULL;
            GameObject explosion = Instantiate(explosionprefab, playerRB.position, Quaternion.identity);
            explosion.GetComponent<Explosion>().isActive = true;
            explosion.GetComponent<Explosion>().fromPlayer = this;
        }

    }

    void Shield()
    {
        if (shieldTimer > 0 && phase == GamePhase.Fly)
        {
            shieldTimer -= Time.deltaTime;
        }
        else
        {
            //activeShield = false;
            shield.SetActive(false);
        }

        // Activate Shield
        if (itemSlot == PowerUp.Type.Shield && (Input.GetKeyDown(KeyCode.Joystick1Button4) || Input.GetKeyDown(KeyCode.F)))
        {
            //activeShield = true;
            shieldTimer = 8;
            shield.SetActive(true);
            itemSlot = PowerUp.Type.NULL;
        }
    }
    #endregion

    #region Commands to sync phases
    private void PutReady()
    {
        CmdPutReady();
    }
    [Command]
    private void CmdPutReady()
    {
        if (!ready)
        {
            this.ready = true;
            hostRocket.GetComponent<ServerScript>().playersReady[pNo - 1] = true;
        }
    }
    private void PutNotReady()
    {
        CmdPutNotReady();
    }
    [Command]
    private void CmdPutNotReady()
    {
        if (ready)
        {
            ready = false;
            hostRocket.GetComponent<ServerScript>().playersReady[pNo - 1] = false;
        }
    }
    #endregion

    #region Fields
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
    
    public PowerUp.Type EquippedItem
    {
        get { return itemSlot; }
    }

    #endregion

    // Just to display the rocket's current phase in the Inspector Window
    public GamePhase currPhase;
}
