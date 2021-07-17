using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private float startTime = 0;
    private bool isStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        Reset();
    }

    public void Reset()
    {
        GetComponent<Text>().text = "0.0";
        isStarted = false;
    }

    public void OnStart()
    {
        startTime = Time.time;
        isStarted = true;
    }

    public void OnFinish()
    {
        isStarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted) {
            double lapTime = Time.time - startTime;
            lapTime = Math.Round(lapTime, 1, MidpointRounding.AwayFromZero);
            GetComponent<Text>().text = lapTime.ToString("F1");
        }
    }
}
