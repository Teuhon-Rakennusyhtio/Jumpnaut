using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickEntityToMe : MonoBehaviour
{
    Vector2 _oldPosition;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            collision.transform.parent = transform;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            collision.transform.parent = null;
        }
    }

    void Update()
    {
        //if (_oldPosition == (Vector2)transform.position) return;
        for (int i = 0; i < transform.childCount; i++)
        {
            Rigidbody2D child = transform.GetChild(i).GetComponent<Rigidbody2D>();
            if (child.velocity.magnitude < 0.95f)
            //child.MovePosition((Vector2)child.position + (Vector2)transform.position - _oldPosition);
            child.position = (Vector2)child.position + (Vector2)transform.position - _oldPosition;
        }
        _oldPosition = transform.position;
    }
}
