using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool CurrentlyInUI = true;
    public static List<ChildDeviceManager> PlayerDevices;
    [SerializeField] Color[] playerColors;

    // -1 means any input device can use the current UI
    public static int UIOwnerId = -1;
    
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        PlayerDevices = new List<ChildDeviceManager>();
    }

    public static Color GetPlayerColor(int id)
    {
        if (id < Instance.playerColors.Length)
        {
            return Instance.playerColors[id];
        }
        Random.InitState(id);
        Color playerColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        Random.InitState((int)System.DateTime.Now.Ticks);
        return playerColor;
    }

    void Update()
    {
        
    }
}
