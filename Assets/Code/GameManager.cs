using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool CurrentlyInUI = true;

    // -1 means any input device can use the current UI
    public static int UIOwnerId = -1;
    
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    void Update()
    {
        
    }
}
