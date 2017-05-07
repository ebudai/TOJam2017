using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogin : MonoBehaviour {

    public string userName;
    // Use this for initialization
    void Awake()
    {
        // Do not destroy this game object:
        DontDestroyOnLoad(this);
    }
}