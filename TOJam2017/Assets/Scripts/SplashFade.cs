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

    void Update()
    {
        if (Input.GetKeyDown("joystick button 0"))
        {
            getNames = true;
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
            
        if (pilotInputField.isFocused && pilotInputField.text != "" && Input.GetKey(KeyCode.Return))
        {
            pilotInputField.enabled = false;
            gunnerInputField.enabled = true;
            gunnerInputField.Select();
        }
        if (gunnerInputField.isFocused && gunnerInputField.text != "" && Input.GetKey(KeyCode.Return))
        {
            loadMain = true;
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
        gunnerInputField.enabled = false;
        pilotInputField.onValueChanged.AddListener(delegate { PilotNameChange(); });
        gunnerInputField.onValueChanged.AddListener(delegate { GunnerNameChange(); });

        uiPanel.SetActive(false);

        splashImage.canvasRenderer.SetAlpha(0.0f);
        FadeIn();

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
                break;

            }
            if (loadMain)
            {
                yield return new WaitForSeconds(1.0f);
                SceneManager.LoadScene(loadLevel);
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
        Debug.Log("Pilot Value Changed");
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
        Debug.Log("Gunner Value Changed");
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
