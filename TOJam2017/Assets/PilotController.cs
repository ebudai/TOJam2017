﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PilotController : MonoBehaviour {

    //private float Thrust { get; set; }
    private float MaxThrust { get; set; }

    private Rigidbody cube;
    
    // Use this for initialization
    void Start () {
        MaxThrust = 100;
        cube = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        HandleRotation();
        HandleThrusting();
	}
    
    private void HandleThrusting()
    {
        var thrust = 40 * Input.GetAxis("Thrust");
        thrust = Math.Min(thrust, MaxThrust);
        thrust = Math.Max(thrust, 0);
        cube.AddForce(transform.forward * thrust);
    }

    private void HandleRotation()
    {
        var rotationAmount = Input.GetAxis("Rotation");
        var verticalAmount = Input.GetAxis("Vertical");
        var horizontalAmount = Input.GetAxis("Horizontal");

        cube.AddTorque(-verticalAmount, horizontalAmount, 0);
        cube.AddRelativeTorque(0, 0, rotationAmount);
    }
}
