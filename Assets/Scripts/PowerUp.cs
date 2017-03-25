using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Type
    {
        Fuel,
        Speed,
        Health
    }

    public Type type;
    public int value;

    // Use this for initialization
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.shader = Shader.Find("Specular");
        if (type == Type.Fuel)
        {
            rend.material.SetColor("_SpecColor", Color.yellow);
            rend.material.color = Color.yellow;
        }
        if (type == Type.Speed)
        {
            rend.material.SetColor("_SpecColor", Color.red);
            rend.material.color = Color.red;
        }
        if (type == Type.Health)
        {
            rend.material.SetColor("_SpecColor", Color.green);
            rend.material.color = Color.green;
        }

    }
}
