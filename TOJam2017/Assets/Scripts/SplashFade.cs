using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashFade : MonoBehaviour {
    public Image splashImage;
    public string loadLevel;
    public InputField pilotInputField;
    public InputField gunnerInputField;
    public GameObject uiPanel;

    private bool getNames = false;
    private bool loadMain = false;
    private bool exit = false;

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            exit = true;
        }

        if (Input.GetKeyDown("joystick button 0"))
        {
            getNames = true;
        }

        if (Input.GetKey(KeyCode.Return))
        {
            if (pilotInputField.text != "")
            {
                if (gunnerInputField.text != "")
                {
                    loadMain = true;
                }
                else
                {
                    pilotInputField.enabled = false;
                    gunnerInputField.enabled = true;
                    gunnerInputField.Select();
                }
            }
        }

        if (pilotInputField.isFocused && pilotInputField.text != "" && Input.GetKey(KeyCode.Return))
        {
            pilotInputField.enabled = false;
            gunnerInputField.enabled = true;
            gunnerInputField.Select();
        }
        if (gunnerInputField.isFocused && gunnerInputField.text == "" && Input.GetKey(KeyCode.Backspace))
        {
            gunnerInputField.enabled = false;
            pilotInputField.enabled = true;
            pilotInputField.Select();
        }
    }

    void Start()
    {
        string text = System.IO.File.ReadAllText("hiscore.txt");
        Debug.Log(text);

        gunnerInputField.enabled = false;
        pilotInputField.onValueChanged.AddListener(delegate { PilotNameChange(); });
        gunnerInputField.onValueChanged.AddListener(delegate { GunnerNameChange(); });

        uiPanel.SetActive(false);

        //splashImage.canvasRenderer.SetAlpha(0.0f);
        //FadeIn();

        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop ()
    {
        while (true)
        {
            if (getNames)
            {
                FadeOut();
                yield return new WaitForSeconds(2.5f);
                uiPanel.SetActive(true);
                pilotInputField.Select();
                getNames = false;
            }
            if (loadMain)
            {
                yield return new WaitForSeconds(1.0f);
                SceneManager.LoadScene(loadLevel);
                break;
            }
            if (exit)
            {
                #if UNITY_EDITOR
                    // Application.Quit() does not work in the editor so
                    // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void FadeIn()
    {
        splashImage.CrossFadeAlpha(1.0f, 1.5f, false);
    }

    void FadeOut()
    {
        splashImage.CrossFadeAlpha(0.0f, 1.5f, false);
    }

    // Invoked when the value of the text field changes.
    public void PilotNameChange()
    {
        var upperText = pilotInputField.text.ToUpper();
        if (upperText != pilotInputField.text) pilotInputField.text = upperText;
        if (pilotInputField.text.Length == 3)
        {
            pilotInputField.enabled = false;
            gunnerInputField.enabled = true;
            gunnerInputField.Select();
        }
    }
    public void GunnerNameChange()
    {
        var upperText = gunnerInputField.text.ToUpper();
        if (upperText != gunnerInputField.text) gunnerInputField.text = upperText;
        if (gunnerInputField.text == "")
        {
            gunnerInputField.enabled = false;
            pilotInputField.enabled = true;
            pilotInputField.Select();
        }
    }
}
