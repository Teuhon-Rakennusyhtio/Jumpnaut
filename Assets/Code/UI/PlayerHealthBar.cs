using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] Sprite _headNormal, _headDamaged, _headDead;
    [SerializeField] Image[] _playerColouredElements;
    [SerializeField] RectTransform _healthRect;
    [SerializeField] RectTransform _durabilityRect;
    [SerializeField] Image _heldItemIcon, _headIcon;
    [SerializeField] GameObject _healthPoint, _durabilityPoint;
    [SerializeField] RectMask2D _analogDurablityMask;
    Color _playerColour, _damagedColour, _deadColour, _currentColour;
    PlayerMover _mover;
    GameObject[] _healthPoints;
    GameObject[] _durabilityPoints;
    int _maxHealth;
    int _currentHealth;
    // Start is called before the first frame update
    void Start()
    {
        SetAnalogDurabilityFullness(0f);
        _heldItemIcon.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AssignPlayer(PlayerMover mover, PlayerHealth health, Color colour)
    {
        StartCoroutine(IEAssignPlayer(mover, health, colour));
    }

    IEnumerator IEAssignPlayer(PlayerMover mover, PlayerHealth health, Color colour)
    {
        _playerColour = colour;
        _damagedColour = Color.Lerp(_playerColour, new Color(0.125f, 0f, 0.25f), 0.2f);
        _deadColour = Color.Lerp(_playerColour, new Color(0.125f, 0f, 0.25f), 0.7f);

        _mover = mover;

        health.PlayerDamaged += OnPlayerDamaged;
        health.PlayerDeath += OnPlayerDeath;
        _healthPoints = new GameObject[health.MaxHealth];
        _maxHealth = health.MaxHealth;
        _currentHealth = _maxHealth;
        for (int i = 0; i < health.MaxHealth; i++)
        {
            _healthPoints[i] = Instantiate(_healthPoint, Vector3.zero, Quaternion.identity, _healthRect);
        }
        ChangeColour(_playerColour);

        yield return new WaitForEndOfFrame();

        foreach (GameObject healthPoint in _healthPoints)
        {
            healthPoint.transform.SetParent(transform);
        }
    }

    void ChangeColour(Color colour)
    {
        _currentColour = colour;
        foreach (Image element in _playerColouredElements)
        {
            element.color = colour;
        }

        for (int i = 0; i < _currentHealth; i++)
        {
            _healthPoints[i].GetComponent<Image>().color = colour;
        }
    }

    public void OnPlayerDamaged(object source, PlayerHealthEventArgs args)
    {
        _headIcon.GetComponent<Image>().sprite = _headDamaged;
        QuickLerpToColour(_damagedColour);
        StartCoroutine(Shake(GetComponent<RectTransform>()));
        Invoke(nameof(ReturnToNormal), 0.5f);
        for (int i = _currentHealth - 1; i >= args.Health; i--)
        {
            _healthPoints[i].GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            StartCoroutine(Shake(_healthPoints[i].GetComponent<RectTransform>()));
        }
        _currentHealth = args.Health;
    }

    public void OnPlayerDeath(object source, PlayerHealthEventArgs args)
    {
        StartCoroutine(Shake(GetComponent<RectTransform>()));
        for (int i = _currentHealth - 1; i >= 0; i--)
        {
            _healthPoints[i].GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            StartCoroutine(Shake(_healthPoints[i].GetComponent<RectTransform>()));
        }
        _currentHealth = 0;
        _headIcon.GetComponent<Image>().sprite = _headDead;
        QuickLerpToColour(_deadColour);
    }

    public void OnItemPickup(object source, WeaponEventArgs args)
    {
        if (args.Sprite != null)
        {
            _heldItemIcon.enabled = true;
            _heldItemIcon.sprite = args.Sprite;
        }
        if (_durabilityPoints != null)
        {
            foreach(GameObject point in _durabilityPoints)
            {
                Destroy(point);
            }
        }
        if (args.Durability > 0)
        {
            _durabilityPoints = new GameObject[args.Durability];
            for (int i = 0; i < _durabilityPoints.Length; i++)
            {
                _durabilityPoints[i] = Instantiate(_durabilityPoint, Vector3.zero, Quaternion.identity, _durabilityRect);
            }
        }
    }

    void ReturnToNormal()
    {
        _headIcon.GetComponent<Image>().sprite = _headNormal;
        QuickLerpToColour(_playerColour);
    }

    IEnumerator Shake(RectTransform rt, float magnitude = 2f)
    {
        Vector3 originalPosition = rt.localPosition;
        float duration = 0.5f;

        while (duration > 0f)
        {
            Vector3 shake = new Vector3(Mathf.Sin(duration * 16), Mathf.Sin(duration * 32) * 2, 0) * magnitude * duration;

            rt.localPosition = originalPosition + shake;
            yield return new WaitForEndOfFrame();
            duration -= Time.deltaTime;
        }

        rt.localPosition = originalPosition;
    }

    void QuickLerpToColour(Color targetColour)
    {
        StartCoroutine(QuickLerpToColourIE(targetColour));
    }

    IEnumerator QuickLerpToColourIE(Color targetColour)
    {
        Color originalColour = _currentColour;
        float duration = 0f;

        while (duration < 0.125f)
        {
            Color newColour = Color.Lerp(originalColour, targetColour, duration * 8);
            ChangeColour(newColour);
            yield return new WaitForEndOfFrame();
            duration += Time.deltaTime;
        }
        ChangeColour(targetColour);
    }

    void SetAnalogDurabilityFullness(float fullness)
    {
        float width = _analogDurablityMask.rectTransform.sizeDelta.x;
        float realFullness = Mathf.Lerp(0, width, 1 - fullness);
        _analogDurablityMask.padding = new Vector4(0, 0, realFullness, 0);
    }
}
