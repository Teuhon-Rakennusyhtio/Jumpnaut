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

    void Start()
    {
        finalTimeText = GetComponent<TextMeshProUGUI>();
        finalTime = GetComponent<SpeedRunTimer>();
        ShowFinalTime();
    } 

    private void ShowFinalTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(finalTime.ReturnTime());
        finalTimeText.text = time.ToString(@"hh\:mm\:ss");
    }
}
