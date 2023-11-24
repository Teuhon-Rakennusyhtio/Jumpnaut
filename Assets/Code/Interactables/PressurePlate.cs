using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] bool _turnedOn;
    [SerializeField] UnityEvent _stepOn, _stepOff;
    [SerializeField] PressurePlateStates _existsInMultiplayerOrSingleplayer = PressurePlateStates.ActiveInBoth;
    uint _howManyThingsAreOnThePlate = 0;
    bool _turnedOnStateIsBeingChanged = false;
    [SerializeField] Color _turnedOnColour, _turnedOffColour;
    Color _targetColour;
    [SerializeField] SpriteRenderer _colouredPart, _glow;
    [SerializeField] Transform _buttonTransform;
    float _buttonLocalHeight;
    // Start is called before the first frame update
    void Start()
    {
        bool singlePlayer = false;
        if (GameManager.Instance != null)
            singlePlayer = GameManager.PlayerDevices.Count == 1;
        
        if ((_existsInMultiplayerOrSingleplayer == PressurePlateStates.ActiveInMultiplayer && singlePlayer)
            || _existsInMultiplayerOrSingleplayer == PressurePlateStates.ActiveInSingleplayer && !singlePlayer)
        {
            Destroy(gameObject);
        }
        _colouredPart.color = _turnedOn ? _turnedOnColour : _turnedOffColour;
        _glow.color = _turnedOn ? _turnedOnColour : Color.black;
        _buttonLocalHeight = _colouredPart.transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPower(bool toggle)
    {
        _turnedOn = toggle;
        _targetColour = _turnedOn ? _turnedOnColour : _turnedOffColour;
        ChangeLookToTurnedOnness();
        if (toggle && _howManyThingsAreOnThePlate > 0)
            _stepOn.Invoke();
        else if (!toggle && _howManyThingsAreOnThePlate > 0)
            _stepOff.Invoke();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (_howManyThingsAreOnThePlate == 0)
            SteppedOn();
        _howManyThingsAreOnThePlate++;
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        _howManyThingsAreOnThePlate--;
        if (_howManyThingsAreOnThePlate == 0)
            SteppedOff();
    }

    void SteppedOn()
    {
        if (_turnedOn)
            _stepOn.Invoke();
        _buttonTransform.localPosition = Vector3.zero;
    }

    void SteppedOff()
    {
        if (_turnedOn)
            _stepOff.Invoke();
        StartCoroutine(ButtonBounceBack());
        //_buttonTransform.localPosition = Vector3.up * 0.1f;
    }

    IEnumerator ButtonBounceBack()
    {
        float doneness = 0f;
        while(_buttonTransform.localPosition.y < 1f)
        {
            if (_howManyThingsAreOnThePlate > 0)
                break;
            doneness += Time.deltaTime * 10;
            if (doneness > 1f)
                doneness = 1f;
            _buttonTransform.localPosition = Vector3.up * _buttonLocalHeight * doneness;
            yield return new WaitForEndOfFrame();
        }
    }

    void ChangeLookToTurnedOnness()
    {
        if (_turnedOnStateIsBeingChanged) return;
        _turnedOnStateIsBeingChanged = true;
        StartCoroutine(ChangingLook());
    }

    IEnumerator ChangingLook()
    {
        Color partCurrentTargetColour = _targetColour;
        Color glowCurrentTargetColour = _targetColour == _turnedOnColour ? _targetColour : Color.black;
        Color partStartingColour = _colouredPart.color;
        Color glowStartingColour = _glow.color;
        float doneness = 0f;
        while (_colouredPart.color != _targetColour)
        {
            doneness += Time.deltaTime * 3f;
            _colouredPart.color = Color.Lerp(partStartingColour, partCurrentTargetColour, doneness);
            _glow.color = Color.Lerp(glowStartingColour, glowCurrentTargetColour, doneness);
            if (partCurrentTargetColour != _targetColour)
            {
                doneness = 0f;
                partStartingColour = _colouredPart.color;
                glowStartingColour = _glow.color;
                partCurrentTargetColour = _targetColour;
                glowCurrentTargetColour = _targetColour == _turnedOnColour ? _targetColour : Color.black;
            }
            yield return new WaitForEndOfFrame();
        }
        _turnedOnStateIsBeingChanged = false;
    }
}

public enum PressurePlateStates
{
    ActiveInBoth,
    ActiveInMultiplayer,
    ActiveInSingleplayer
}
