using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject _start, _joinGame, _settings;
    GameObject _lastSelectedButtonInStart;
    // Start is called before the first frame update
    void Start()
    {
        ToggleAllButtons(_joinGame, false);
        ToggleAllButtons(_settings, false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ToggleAllButtons(GameObject subMenu, bool toggle)
    {
        Button[] buttons = subMenu.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = toggle;
        }
    }

    public void ClickedQuit() => Application.Quit();

    [SerializeField] TextMeshProUGUI statement, agree;
    public void GoToSettingsScreen()
    {
        string[] opinions = {
                            "Pineapple is the best pizza topping.",
                            "I am right, you are wrong.",
                            $"{Random.Range(0, 99)} is the best number.",
                            "It's obviously blue and black.",
                            "This game is just so perfect, you don't need to change the settings.",
                            "Anyone who doesn't think it's white and gold is a fool.",
                            "Violence is the answer!",
                            "I know what is best for you because I am you.",
                            "I don't know you and I don't care to know you.",
                            "You should get a job.",
                            "These donuts are great! Jelly filled are my favorite. Nothing beats a jelly filled donut!",
                            "Keep going you're so close!",
                            "Anime. It's the " + (Random.Range(0, 2) == 1 ? "best" : "worst") + " isn't it?"
                            };
        string[] answers = {
                            "Yes",
                            "I agree",
                            "Indeed, I believe so",
                            "So true!",
                            "Uh huh",
                            "Ye"
                            };
        statement.text = opinions[Random.Range(0, opinions.Length)];
        agree.text = answers[Random.Range(0, answers.Length)];
        _lastSelectedButtonInStart = EventSystem.current.currentSelectedGameObject;
        ToggleAllButtons(_start, false);
        _start.GetComponent<Animator>().Play("Out");
        _settings.GetComponent<Animator>().Play("In");
    }

    public void GoToJoinGameScreen()
    {
        _lastSelectedButtonInStart = EventSystem.current.currentSelectedGameObject;
        ToggleAllButtons(_start, false);
        _start.GetComponent<Animator>().Play("Out");
        _joinGame.GetComponent<Animator>().Play("In");
    }

    public void ReturnFromJoinGameScreen()
    {
        ToggleAllButtons(_joinGame, false);
        _start.GetComponent<Animator>().Play("In");
        _joinGame.GetComponent<Animator>().Play("Out");
    }

    public void ReturnFromSettingsScreen()
    {
        ToggleAllButtons(_settings, false);
        _start.GetComponent<Animator>().Play("In");
        _settings.GetComponent<Animator>().Play("Out");
    }

    public void StartScreenReturnAnimation()
    {
        ToggleAllButtons(_start, true);
        EventSystem.current.SetSelectedGameObject(_lastSelectedButtonInStart);
    }
}
