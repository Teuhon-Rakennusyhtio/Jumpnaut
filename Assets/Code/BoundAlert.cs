using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundAlert : MonoBehaviour
{    
    public GameObject icon;
    public SpriteRenderer iconRenderer;
        
    void Start()
    {
        icon = transform.gameObject;
        iconRenderer = icon.GetComponent<SpriteRenderer>();
        iconRenderer.enabled = false;
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.tag == "Edge")
        {
            iconRenderer.enabled = true;
        }
        else
        {
            iconRenderer.enabled = false;
        }
    }
}
