using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool CurrentlyInUI = true;
    public static List<ChildDeviceManager> PlayerDevices;
    [SerializeField] Color[] playerColors;

    // -1 means any input device can use the current UI
    public static int UIOwnerId = -1;


    // Settings
    public static float ShakeIntensity = 1;
    
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        PlayerDevices = new List<ChildDeviceManager>();
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            ReturnToMainMenu();
        }
        else
        {
            CurrentlyInUI = false;
        }
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

    public static void ReturnToMainMenu()
    {
        GameManager.CurrentlyInUI = true;
        UIOwnerId = -1;
        Camera.main.GetComponent<CameraMovement>().ReturnToMainMenu();
        PlayerDevices.Clear();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    void Update()
    {
        
    }
}
