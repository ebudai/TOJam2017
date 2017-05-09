using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private bool hiscoreShown = false;
    void Update()
    {
        if (hiscoreShown && Input.GetKeyDown("joystick button 7"))
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
        string[] lines = hiscoreText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        Dictionary<string, int> scoreTable = new Dictionary<string, int>();
        int i;

        for (i=0;i<lines.Length; i++)
        {
            string thisLine = lines[i];
            if (thisLine == "HIGH SCORES") continue;
            string[] vals = thisLine.Split(' ');
            //string[] lines = hiscoreText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            scoreTable[vals[0]] = Int32.Parse(vals[1]);
        }
        GameObject persistentGameObject = GameObject.Find("PlayerInfoStore");
        var persistentScript = persistentGameObject.GetComponent<PlayerInfo>();
        scoreTable[persistentScript.playerName] = persistentScript.score;

        string newHiScores = "HIGH SCORES";
        var ordered = scoreTable.OrderByDescending(x => x.Value).Take(5);
        foreach (var kvp in ordered)
        {
            newHiScores += "\n" + kvp.Key + " " + kvp.Value.ToString();
        }
        System.IO.File.WriteAllText("hiscore.txt", newHiScores);
        gameOverImg.canvasRenderer.SetAlpha(0.0f);
        hiScoreField.text = newHiScores;
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
        hiscoreShown = true;
    }
}
