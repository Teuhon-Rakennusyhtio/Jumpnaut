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
    public static SpeedRunTimer SpeedTimer;

    // -1 means any input device can use the current UI
    public static int UIOwnerId = -1;


    // Settings
    public static float ShakeIntensity = 1;


    // Ladder Song
    public static AudioClip LadderSongClip;

    // The save file
    public static SaveFile SaveFile;
    
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        PlayerDevices = new List<ChildDeviceManager>();
        SaveFile = SaverLoader.ReadSaveFromBinary();
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            ReturnToMainMenu();
        }
        else
        {
            CurrentlyInUI = false;
        }
        SpeedTimer = GetComponent<SpeedRunTimer>();
        StartCoroutine(LadderSong.GetSong());
        Debug.Log(SaveFile.HighScore);
        Debug.Log(SaveFile.LowestTime);
        Debug.Log(SaveFile.CurrentRunCheckPointPosition);
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

    public static void SaveToFile()
    {
        SaverLoader.WriteSaveToBinary(SaveFile);
    }

    public static void StartGame(bool newGame = false)
    {
        CurrentlyInUI = false;
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        if (!newGame)
        {
            score = SaveFile.CurrentRunScore;
            if (score < 0)
            {
                score = 0;
            }
        }
        else
        {
            ClearRunFromTheSave();
        }
        SpeedRunTimer.SetTime(SaveFile.CurrentRunTime);
    }

    public static void ReturnToMainMenu()
    {
        SaveToFile();
        CurrentlyInUI = true;
        UIOwnerId = -1;
        Camera.main.GetComponent<CameraMovement>().ReturnToMainMenu();
        PlayerDevices.Clear();
        score = 0;
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
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
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
        SaveFile.CurrentRunScore += points;
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

    public static void ClearRunFromTheSave()
    {
        SaveFile.CurrentRunScore = 0;
        SaveFile.CurrentRunTime = 0f;
        SaveFile.CurrentRunCheckPointPosition = Vector2.negativeInfinity;
    }

    void Update()
    {
        
    }
}
