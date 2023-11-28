using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class WinerWinnerChickenDinner : MonoBehaviour
{
    [SerializeField] GameObject _playAgain, _quit;
    bool _newHighScore, _newLowestTime;
    void Start()
    {
        GameManager.CurrentlyInUI = true;
        EventSystem.current.SetSelectedGameObject(_playAgain);

        _newHighScore = GameManager.SaveFile.HighScore < GameManager.SaveFile.CurrentRunScore;
        _newLowestTime = GameManager.SaveFile.LowestTime > 0f && GameManager.SaveFile.CurrentRunTime < GameManager.SaveFile.LowestTime;

        if (_newHighScore)
        {
            GameManager.SaveFile.HighScore = GameManager.SaveFile.CurrentRunScore;
        }
        if (_newLowestTime)
        {
            GameManager.SaveFile.LowestTime = GameManager.SaveFile.CurrentRunTime;
        }


        GameManager.ClearRunFromTheSave();
    }
    public void PlayAgain()
    {
        Camera.main.GetComponent<CameraMovement>().ReturnToMainMenu();
        GameManager.CurrentlyInUI = false;
        GameManager.score = 0;
        PlayerPrefs.DeleteKey("FinalTime");
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainScene");
        //GameManager.ReloadMainScene(SceneManager.GetSceneByName("WinScene"));
        //StartCoroutine(IEPlayAgainGlitch());
    }

    /*IEnumerator IEPlayAgainGlitch()
    {
        yield return new WaitUntil( () => GameManager.MainSceneReloaded);
        SceneManager.UnloadSceneAsync("WinScene");
    }*/

    public void QuitToMainMenu()
    {
        GameManager.ReturnToMainMenu();
    }
}
