﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public string playerName;
    public int score;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
