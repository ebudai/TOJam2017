using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrabFaceAnimation : MonoBehaviour {
    private AudioSource crabVoice;
    public Sprite[] frames;
    private Image display;
    private int frameNum = 0;
    private float startTime;
    private bool playing = false;

    private void Awake()
    {
        crabVoice = GetComponent<AudioSource>();
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
        crabVoice.Play();
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
            if (Time.time - startTime > 2.4)
            {
                done = true;
            }
            yield return new WaitForSeconds(0.1f);
        }
        StopAnimation();
    }

    private void StopAnimation()
    {
        playing = false;
        display.enabled = false;
    }
}
