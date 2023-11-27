using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedRunTimer : MonoBehaviour
{
    bool stopwatchActive = true;
    float currentTime;
    float finalTime;
    TextMeshProUGUI currentTimeText;

    void Start()
    {
        currentTimeText = GetComponent<TextMeshProUGUI>();
        currentTime = 0;
    }

    void Update()
    {
        if (stopwatchActive == true)
        {
            currentTime = currentTime + Time.deltaTime;
            TimeSpan time = TimeSpan.FromSeconds(currentTime);
            currentTimeText.text = time.ToString(@"hh\:mm\:ss");
        }
    }

    public void StopTimer()
    {
        stopwatchActive = false;
        finalTime = currentTime;
        PlayerPrefs.SetFloat("FinalTime", finalTime);
    }

    public void StartTimer()
    {
        stopwatchActive = true;
    }

    public float ReturnTime()
    {
        return finalTime;
    }
}
