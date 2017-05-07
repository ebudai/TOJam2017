using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndCredits : MonoBehaviour
{
    public Image gameOverImg;
    public string loadLevel;
    public GameObject hiScores;
    public GameObject creditsPanel;
    public Text hiScoreField;

    private bool exit = false;
    private bool gotoSplash = false;

    void Update()
    {
        if (Input.GetKeyDown("joystick button 0"))
        {
            gotoSplash = true;
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            exit = true;
        }
    }

    void Start()
    {
        string hiscoreText = System.IO.File.ReadAllText("hiscore.txt");
        gameOverImg.canvasRenderer.SetAlpha(0.0f);
        //creditsImg.canvasRenderer.SetAlpha(0.0f);

        hiScoreField.text = hiscoreText;
        hiScores.SetActive(false);

        StartCoroutine(ExitLoop());
        StartCoroutine(LoadCredits());
    }

    IEnumerator ExitLoop()
    {
        while (true)
        {
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
            if (gotoSplash)
            {
                SceneManager.LoadScene(loadLevel);
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void FadeInEnd()
    {
        gameOverImg.CrossFadeAlpha(1.0f, 1.5f, false);
    }

    void FadeOutEnd()
    {
        gameOverImg.CrossFadeAlpha(0.0f, 1.5f, false);
    }

    void FadeInCredits()
    {
        creditsPanel.SetActive(true);
    }

    void FadeOutCredits()
    {
        creditsPanel.SetActive(false);
    }

    IEnumerator LoadCredits()
    {
        FadeInEnd();
        yield return new WaitForSeconds(2.0f);
        FadeOutEnd();
        yield return new WaitForSeconds(2.0f);
        //FadeInCredits();
        creditsPanel.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        //FadeOutCredits();
        creditsPanel.SetActive(false);
        yield return new WaitForSeconds(2.0f);
        hiScores.SetActive(true);
    }
}
