using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EelFaceAnim : MonoBehaviour
{
    private AudioSource eelVoice;
    public Sprite[] frames;
    private Image display;
    private int frameNum = 0;
    private float startTime;
    private bool playing = false;

    private void Awake()
    {
        eelVoice = GetComponent<AudioSource>();
        display = GetComponent<Image>();
        display.enabled = false;
    }

    public void Play()
    {
        if (playing) return;
        playing = true;
        startTime = Time.time;
        display.enabled = true;

        //play sound
        eelVoice.Play();
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        bool done = false;
        while (!done)
        {
            GetComponent<Image>().sprite = frames[frameNum];
            frameNum++;
            if (frameNum >= frames.Length)
            {
                frameNum = 0;
            }
            //check for stop
            if (Time.time - startTime > 2.9)
            {
                done = true;
            }
            yield return new WaitForSeconds(0.2f);
        }
        StopAnimation();
    }

    private void StopAnimation()
    {
        playing = false;
        display.enabled = false;
    }
}
