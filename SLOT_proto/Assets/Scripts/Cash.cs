using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cash : MonoBehaviour
{
    Text cash;
    VariableStorage cashAmount;
    GameObject dialogue;


	// Use this for initialization
	void Awake ()
    {
        cash = GetComponent<Text>();
        dialogue = GameObject.Find("Dialogue");
        cashAmount = dialogue.GetComponent<VariableStorage>();
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        cash.text = cashAmount.GetValue("$cash").AsString;
	}
}
