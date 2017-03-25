using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Script : MonoBehaviour {

    public PlayerBehaviour playerScript;
    public RectTransform fuelArrow;
    public RectTransform fuelLimitArrow;

    public Text countdownText;
    public Text ammoCount;

    public Image healthBar;
    public Text healthText;

    public GameObject hostPlayer;

    private float countdownTextTimer;

    private const float maxHealthOffset = -318;

    void Start()
    {
        countdownTextTimer = 1;

        fuelArrow = GameObject.Find("FM Arrow").GetComponent<RectTransform>();
        fuelLimitArrow = GameObject.Find("FM Launch Limit").GetComponent<RectTransform>();
        countdownText =  GameObject.Find("Countdown").GetComponent<Text>();
        ammoCount = GameObject.Find("Ammo Count").GetComponent<Text>();
        healthBar = GameObject.Find("Health Bar Bar").GetComponent<Image>();
        healthText = GameObject.Find("Health Bar Text").GetComponent<Text>();
    }

    void Update()
    {
        FuelMeter();
        
        if (playerScript.Countdown != 0)
        {
            countdownText.text = "" + playerScript.Countdown;
            countdownTextTimer = 1;
        }
        else if(countdownTextTimer > 0)
        {
            countdownText.text = "LAUNCH!";
            countdownTextTimer -= Time.deltaTime;
        }
        else
        {
            countdownText.text = "";
        }

        ammoCount.text = "Ammo:\n" + playerScript.Magazine + "/" + playerScript.magazineSize + "\n" + playerScript.ammo;

        float healthPercentage = playerScript.CurrHealth / playerScript.maxHealth;
        
        Vector3 healthPos = healthBar.rectTransform.position;
        float xPos = (1 - healthPercentage) * maxHealthOffset;
        healthBar.rectTransform.position = new Vector3(xPos, healthPos.y, healthPos.z);
        
        healthText.text =  Mathf.Max ((int)(healthPercentage * 100), 0) + "%";

        Color c = new Color((1 - healthPercentage), healthPercentage, 0);
        healthBar.color = c;
        healthText.color = c;
    }

    private void FuelMeter()
    {
        if (playerScript.CurrentPhase == PlayerBehaviour.GamePhase.Fly)
        {
            float fuelArrowAngle = -playerScript.fuel / 100 * 360;
            fuelArrow.rotation = Quaternion.Euler(0, 0, fuelArrowAngle);

            // Display the fuel stand that the player had right after launch
            float maxFuelArrowAngle = -playerScript.FuelAfter / 100 * 360;
            fuelLimitArrow.rotation = Quaternion.Euler(0, 0, maxFuelArrowAngle);
        }
        else if (playerScript.CurrentPhase == PlayerBehaviour.GamePhase.Launch)
        {
            float fuelArrowAngle = -(playerScript.FuelAfter) / 100 * 360;
            fuelArrow.rotation = Quaternion.Euler(0, 0, fuelArrowAngle);
            
            // Display the max amount of fuel the player can use for launch
            float hypotheticalFuel = playerScript.fuel - playerScript.maxLaunchFuel;
            float maxFuelArrowAngle = -(Mathf.Max(0, hypotheticalFuel)) / 100 * 360;
            fuelLimitArrow.rotation = Quaternion.Euler(0, 0, maxFuelArrowAngle);
        }
    }
}
