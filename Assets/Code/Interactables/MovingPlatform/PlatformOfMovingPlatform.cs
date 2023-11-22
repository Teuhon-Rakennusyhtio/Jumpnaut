using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformOfMovingPlatform : MonoBehaviour
{
    MovingPlatform _movingPlatform;
    Rigidbody2D _rigidbody;
    List<Collider2D> _attachedColliders;
    List<int> _removeTheseFromTheList;

    void Start()
    {
        _movingPlatform = transform.parent.GetComponent<MovingPlatform>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _attachedColliders = new List<Collider2D>();
        _removeTheseFromTheList = new List<int>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        _movingPlatform.OnCollisionChange(true, collision);
        _attachedColliders.Add(collision.collider);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        _movingPlatform.OnCollisionChange(false, collision);
        _attachedColliders.Remove(collision.collider);
    }

    void Update()
    {
        if (Time.frameCount % 60 == 0)
            FixAttachedCollidersList();
    }

    void FixAttachedCollidersList()
    {
        _removeTheseFromTheList.Clear();
        foreach (Collider2D collider in _attachedColliders)
        {
            if (!_rigidbody.IsTouching(collider))
            {
                //_attachedColliders.Remove(collider);
                _removeTheseFromTheList.Add(_attachedColliders.IndexOf(collider));
                _movingPlatform.OnCollisionChange(false, collider);
                Debug.Log($"{collider.gameObject.name} was removed from {_movingPlatform.gameObject.name}'s rigidbody list");
            }
        }
        foreach (int index in _removeTheseFromTheList)
        {
            _attachedColliders.RemoveAt(index);
        }
    }
}
