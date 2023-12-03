using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoinGameSubMenu : MonoBehaviour
{
    string _readyText = "players ready";
    float _uiDelay = 0f;
    public bool CurrentlyInJoinGame = false;
    int _currentPlayerCount = 0, _currentReadyPlayerCount = 0;
    int _latestInputId;
    int _selectedButton = 0;
    bool[] _inputs;
    List<int> _currentlyJoinedPlayers;
    List<PlayerJoinIcon> _playerIcons;
    [SerializeField] PlayerJoinIcon _firstPlayerIcon;
    [SerializeField] GameObject _playerIconPrefab;
    [SerializeField] Transform _iconField;
    [SerializeField] TextMeshProUGUI _readyPlayersText;
    [SerializeField] Button _newGameButton, _continueGameButton;

    // Start is called before the first frame update
    void Start()
    {
        _currentlyJoinedPlayers = new List<int>();
        _playerIcons = new List<PlayerJoinIcon>();
        _playerIcons.Add(_firstPlayerIcon);
        SetReadyPlayersText();
    }

    // Update is called once per frame
    void Update()
    {
        if (!CurrentlyInJoinGame) return;

        _uiDelay -= Time.deltaTime;
        _inputs = MainDeviceManager.GetLatestInputs();
        _latestInputId = MainDeviceManager.GetLatestInputId();



        if (_inputs[(int) ChildDeviceManager.InputTypes.cancel] && GameManager.PlayerDevices.Count == 0 && _uiDelay <= 0f)
        {
            BackToStart();
        }
        else if (_inputs[(int) ChildDeviceManager.InputTypes.confirm] && !_currentlyJoinedPlayers.Contains(_latestInputId))
        {
            PlayerJoined();
        }
    }

    void SetReadyPlayersText()
    {
        _readyPlayersText.text = _currentPlayerCount > 0 ? $"{_currentReadyPlayerCount} / {_currentPlayerCount} {_readyText}" : "";
        if (!CurrentlyInJoinGame) return;
        foreach (PlayerJoinIcon icon in _playerIcons)
        {
            icon.JoinGameUpdated();
        }
        if (_currentReadyPlayerCount == _currentPlayerCount && _currentPlayerCount > 0)
        {
            StartCoroutine(EnableStartGameButtons());
            //GameManager.StartGame();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            _newGameButton.interactable = false;
            _continueGameButton.interactable = false;
        }
    }

    IEnumerator EnableStartGameButtons()
    {
        yield return new WaitForSeconds(0.2f);
        _newGameButton.interactable = true;
        if (GameManager.SaveFile.CurrentRunTime > 0.01f)
        {
            _continueGameButton.interactable = true;
        }

        if (_selectedButton == 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_newGameButton.gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_continueGameButton.gameObject);
        }
    }

    public void StartNewGame()
    {
        GameManager.ClearRunFromTheSave();
        GameManager.StartGame();
    }

    public void ContinueGame()
    {
        GameManager.StartGame();
    }

    void PlayerJoined()
    {
        GameManager.PlayerDevices.Add(MainDeviceManager.GetChildDeviceManager(_latestInputId));
        _currentlyJoinedPlayers.Add(_latestInputId);
        _playerIcons[_currentPlayerCount].AssignPlayer(MainDeviceManager.GetChildDeviceManager(_latestInputId));
        _playerIcons.Add(Instantiate(_playerIconPrefab, _iconField).GetComponent<PlayerJoinIcon>());
        _currentPlayerCount++;
        SetReadyPlayersText();
    }

    public void PlayerLeft(int id, PlayerJoinIcon icon)
    {
        GameManager.PlayerDevices.Remove(MainDeviceManager.GetChildDeviceManager(id));
        _playerIcons.Remove(icon);
        _currentlyJoinedPlayers.Remove(id);
        _currentPlayerCount--;
        _uiDelay = 0.2f;
        SetReadyPlayersText();
    }

    public void PlayerReady(bool toggle)
    {
        _currentReadyPlayerCount += toggle ? 1 : -1;
        SetReadyPlayersText();
    }

    public int GetPlayerJoinIconPosition(PlayerJoinIcon icon) => _playerIcons.IndexOf(icon);

    void BackToStart()
    {
        gameObject.GetComponentInParent<MainMenu>().ReturnFromJoinGameScreen();
    }
}
