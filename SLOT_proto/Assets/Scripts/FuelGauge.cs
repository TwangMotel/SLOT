using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FuelGauge : MonoBehaviour
{
    [Range(-180.0f, 0.0f)]
    public float fuelLevel;
    GameObject needle;

    public float fuelEfficiency;

    private void Start()
    {
         needle = GameObject.Find("needle");
        fuelLevel = 0f;
        fuelEfficiency = 8f;
    }

    public void Update()
    {
        needle.transform.eulerAngles = new Vector3(0f, 0f, -fuelLevel);
    }

}
