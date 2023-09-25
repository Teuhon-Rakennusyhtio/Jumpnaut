using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    public bool _oneTimeUse = false;
    float _useTipBubbleYCoord;
    [SerializeField] UnityEvent _pullLeft, _pullRight;
    [SerializeField] Animator _anim;
    [SerializeField] SpriteRenderer _useTipBubbleGraphic;
    Transform _useTipBubble;
    List<PlayerMover> _movers;
    bool _currentDirection = false, _alreadyPulled = false, _pull = false;
    
    void Start()
    {
        _useTipBubble = _useTipBubbleGraphic.gameObject.transform;
        _useTipBubbleYCoord = _useTipBubble.position.y;
        _movers = new List<PlayerMover>();
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
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (_movers.Count == 0 ||
            _oneTimeUse && _currentDirection) return;
        
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