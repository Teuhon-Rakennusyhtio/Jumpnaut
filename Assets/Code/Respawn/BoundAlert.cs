using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundAlert : MonoBehaviour
{    
    public SpriteRenderer iconRenderer;
        
    void Start()
    {
        iconRenderer = GetComponent<SpriteRenderer>();
        iconRenderer.enabled = false;
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.name == "BoundAlert")
        {
            iconRenderer.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        iconRenderer.enabled = false;
    }
}
