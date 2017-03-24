using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Script : MonoBehaviour {

    public PlayerBehaviour playerScript;
    public RectTransform fuelArrow;
    public RectTransform fuelLimitArrow;

    public Text countdownText;

    private float countdownTextTimer;

    void Start()
    {
        countdownTextTimer = 1;
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
