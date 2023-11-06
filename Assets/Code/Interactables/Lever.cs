using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    public bool _oneTimeUse = false;
    float _useTipBubbleYCoord;
    [SerializeField] UnityEvent _pullLeft, _pullRight, _whenInSinglePlayer;
    [SerializeField] bool _goneInSinglePlayer;
    //[SerializeField] float _automaticDelay = 2f;
    [SerializeField] Animator _anim;
    [SerializeField] SpriteRenderer _useTipBubbleGraphic;
    Transform _useTipBubble;
    List<PlayerMover> _movers;
    bool _currentDirection = false, _alreadyPulled = false, _pull = false, _singlePlayer;
    
    void Start()
    {
        _useTipBubble = _useTipBubbleGraphic.gameObject.transform;
        _useTipBubbleYCoord = _useTipBubble.position.y;
        _movers = new List<PlayerMover>();
        _singlePlayer = GameManager.PlayerDevices.Count == 1 && _goneInSinglePlayer;

        if (_singlePlayer)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            _whenInSinglePlayer.Invoke();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMover mover = collision.gameObject.GetComponent<PlayerMover>();
        if (mover != null)
        {
            _movers.Add(mover);
            UpdateBubblePosition();
            _useTipBubbleGraphic.enabled = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMover mover;
        mover = collision.gameObject.GetComponent<PlayerMover>();
        if (mover != null) _movers.Remove(mover);
        _useTipBubbleGraphic.enabled = _movers.Count != 0;
    }

    void Update()
    {
        _pull = false;
        /*if (_singlePlayer)
        {
            if (_delay > _automaticDelay)
            {
                _delay = 0f;
                Pull();
            }
            _delay += Time.deltaTime;
        }*/
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (_movers.Count == 0 ||
            _oneTimeUse && _currentDirection ||
            _singlePlayer) return;
        
        if (!_pull)
        {
            foreach (PlayerMover mover in _movers)
            {
                if (mover.MoveInput.y > 0.3f || _pull) _pull = true;
            }
        }
        if (!_alreadyPulled && _pull)
        {
            _alreadyPulled = true;
            Pull();
        }
        else if (_alreadyPulled && !_pull)
        {
            _alreadyPulled = false;
        }

        UpdateBubblePosition();
    }

    void UpdateBubblePosition()
    {
        _useTipBubble.position =
            new Vector2(_useTipBubble.position.x,
            _useTipBubbleYCoord +
            Mathf.Sin(Time.timeSinceLevelLoad * 3) * 0.1f);
    }

    void Pull()
    {
        if (_currentDirection)
        {
            _currentDirection = false;
            _anim.Play("PullToLeft");
            _pullLeft.Invoke();
        }
        else
        {
            _currentDirection = true;
            _anim.Play("PullToRight");
            _pullRight.Invoke();
        }
    }
}