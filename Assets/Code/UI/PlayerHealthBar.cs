using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] Image[] _playerColouredElements;
    [SerializeField] RectTransform _healthRect;
    [SerializeField] RectTransform _durabilityRect;
    [SerializeField] Image _heldItemIcon;
    [SerializeField] GameObject _healthPoint, _durabilityPoint;
    Color _playerColour;
    PlayerMover _mover;
    PlayerHealth _health;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AssignPlayer(PlayerMover mover, PlayerHealth health, Color colour)
    {
        _playerColour = colour;
        _mover = mover;
        _health = health;

        foreach (Image element in _playerColouredElements)
        {
            element.color = _playerColour;
        }
    }
}
