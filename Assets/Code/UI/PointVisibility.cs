using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointVisibility : MonoBehaviour
{
    private TextMeshProUGUI TexMexi;
    // Start is called before the first frame update
    void Start()
    {
        TexMexi = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        TexMexi.text = $"{GameManager.DisplayScore()}";
    }
}
