using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        var player = GameObject.Find("PlayerShip");
        transform.position = player.transform.position + new Vector3(28f, 106f, -20.4f);
    }
}
