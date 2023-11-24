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
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    public void QuitToMainMenu()
    {
        GameManager.ReturnToMainMenu();
    }
}
