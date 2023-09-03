using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;
    ChildDeviceManager _device;
    Button[] _buttons;
    //float _uiDelay = 0f;
    //bool[] _inputs;
    bool _paused = false;
    [SerializeField] Image _playerNumberBackground;
    [SerializeField] TextMeshProUGUI _playerNumberText;
    [SerializeField] GameObject _resumeButton;
    Animator _animator;


    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        _animator = GetComponent<Animator>();
        _buttons = gameObject.GetComponentsInChildren<Button>();
        foreach (Button button in _buttons)
        {
            button.interactable = false;
        }
    }

    void Update()
    {
        //if (_uiDelay > 0f) _uiDelay -= Time.unscaledDeltaTime;



        /*if (_inputs[(int) ChildDeviceManager.InputTypes.pause] && _uiDelay <= 0f)
        {
            BackToGame();
        }*/
    }

    public void BackToGame()
    {
        foreach (Button button in _buttons)
        {
            button.interactable = false;
        }
        _animator.Play("Out");
    }

    public void BackToMainMenu()
    {
        GameManager.ReturnToMainMenu();
    }

    public static void Open(PlayerMover player)
    {
        if (Instance._paused) return;
        Instance._paused = true;
        GameManager.CurrentlyInUI = true;
        Time.timeScale = 0.0f;
        Instance._device = player.Device;
        Instance._playerNumberText.text = $"P{player.Id + 1}";
        Instance._playerNumberBackground.color = GameManager.GetPlayerColor(player.Id);
        GameManager.UIOwnerId = Instance._device.Id;
        Instance._animator.Play("In");
    }

    public void OpenPauseAnimation()
    {
        foreach (Button button in _buttons)
        {
            button.interactable = true;
        }
        EventSystem.current.SetSelectedGameObject(_resumeButton);
    }

    public void ClosePauseAnimation()
    {
        Time.timeScale = 1.0f;
        _paused = false;
        GameManager.CurrentlyInUI = false;
    }
}
