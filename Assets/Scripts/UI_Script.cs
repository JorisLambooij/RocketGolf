using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Script : MonoBehaviour {

    public PlayerBehaviour playerScript;
    public RectTransform fuelArrow;
    public RectTransform fuelLimitArrow;
    
	void Update ()
    {
        float arrowAngle = -playerScript.fuel / 100 * 360;
        fuelArrow.rotation = Quaternion.Euler(0, 0, arrowAngle);

        arrowAngle = -(100 - playerScript.maxLaunchFuel) / 100 * 360;
        fuelLimitArrow.rotation = Quaternion.Euler(0, 0, arrowAngle);
    }
}
