using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class WinerWinnerChickenDinner : MonoBehaviour
{
    [SerializeField] GameObject _playAgain, _quit;
    void Start()
    {
        GameManager.CurrentlyInUI = true;
        EventSystem.current.SetSelectedGameObject(_playAgain);
    }
    public void PlayAgain()
    {
        Camera.main.GetComponent<CameraMovement>().ReturnToMainMenu();
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
