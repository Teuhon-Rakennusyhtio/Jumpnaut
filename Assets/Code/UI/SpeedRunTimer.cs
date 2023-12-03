using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedRunTimer : MonoBehaviour
{
    bool stopwatchActive = true;
    static float currentTime;
    float finalTime;
    TextMeshProUGUI currentTimeText;

    void Start()
    {
        currentTimeText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (stopwatchActive == true)
        {
            currentTime = currentTime + Time.deltaTime;
            GameManager.SaveFile.CurrentRunTime = currentTime;
            TimeSpan time = TimeSpan.FromSeconds(currentTime);
            currentTimeText.text = time.ToString(@"hh\:mm\:ss");
        }
    }

    public void StopTimer()
    {
        stopwatchActive = false;
        finalTime = currentTime;
        PlayerPrefs.SetFloat("FinalTime", finalTime);
        currentTime = 0;
    }

    public void StartTimer()
    {
        stopwatchActive = true;
    }

    public static void SetTime(float time)
    {
        currentTime = time;
    }

    public float ReturnTime()
    {
        return finalTime;
    }
}
