using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    public bool _oneTimeUse = false;
    [SerializeField] UnityEvent _pullLeft, _pullRight;
    [SerializeField] Animator _anim;
    List<PlayerMover> _movers;
    bool _currentDirection = false, _alreadyPulled = false, _pull = false;
    
    void Start()
    {
        _movers = new List<PlayerMover>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMover mover = collision.gameObject.GetComponent<PlayerMover>();
        if (mover != null) _movers.Add(mover);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMover mover = collision.gameObject.GetComponent<PlayerMover>();
        if (mover != null) _movers.Remove(mover);
    }

    void Update()
    {
        _pull = false;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (_movers.Count == 0 || _oneTimeUse && _currentDirection) return;
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