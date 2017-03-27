using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Type
    {
        Fuel,
        Speed,
        Health,
        Bomb,
        Shield,
        NULL
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
        if (type == Type.Bomb)
        {
            rend.material.SetColor("_SpecColor", Color.magenta);
            rend.material.color = Color.magenta;
        }
        if (type == Type.Shield)
        {
            rend.material.SetColor("_SpecColor", Color.cyan);
            rend.material.color = Color.cyan;
        }

    }
    void Update()
    {
        this.transform.RotateAround(this.transform.position, this.transform.up, 1);
        this.transform.RotateAround(this.transform.position, this.transform.right, 1);
        this.transform.RotateAround(this.transform.position, this.transform.up, 1);
    }
}
