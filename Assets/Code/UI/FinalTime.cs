using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinalTime : MonoBehaviour
{
    SpeedRunTimer finalTime;
    TextMeshProUGUI finalTimeText;
    float gottenFinalTime;

    void Start()
    {
        gottenFinalTime = PlayerPrefs.GetFloat("FinalTime", 0f);
        finalTimeText = GetComponent<TextMeshProUGUI>();
        finalTime = GetComponent<SpeedRunTimer>();
        ShowFinalTime();
    } 

    private void ShowFinalTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(gottenFinalTime);
        finalTimeText.text = time.ToString(@"hh\:mm\:ss");
    }
}
