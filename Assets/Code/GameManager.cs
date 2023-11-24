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
    public static int score;
    public static bool MainSceneReloaded;

    // -1 means any input device can use the current UI
    public static int UIOwnerId = -1;


    // Settings
    public static float ShakeIntensity = 1;


    // Ladder Song
    public static AudioClip LadderSongClip;
    
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
        
        StartCoroutine(LadderSong.GetSong());
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

    public static void ReloadMainScene(Scene bufferScene)
    {
        MainSceneReloaded = false;
        Instance.StartCoroutine(Instance.IEReloadMainScene(bufferScene));
    }

    IEnumerator IEReloadMainScene(Scene bufferScene)
    {
        SceneManager.SetActiveScene(bufferScene);
        
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync("MainScene");
        while (!asyncUnload.isDone)
        {
            yield return null;
        }
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainScene"));
        MainSceneReloaded = true;
    }

    public static void AddScore(int points, Vector3 position)
    {
        score += points;
        GameObject scoreGraphic = Instantiate(Resources.Load<GameObject>("Prefabs/ScoreGraphic"), position, Quaternion.identity);
        scoreGraphic.GetComponent<NewScoreGraphic>().GotScore(points);
        if (score < 0)
        {
            score = 0;
        }
    }

    public static int DisplayScore()
    {
        return score;
    }

    void Update()
    {
        
    }
}
