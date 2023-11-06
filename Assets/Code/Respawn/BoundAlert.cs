using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundAlert : MonoBehaviour
{    
    public SpriteRenderer iconRenderer;
    bool respawning;
        
    void Start()
    {
        iconRenderer = GetComponent<SpriteRenderer>();
        iconRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.name == "Main Camera")
        {
            respawning = true;
        }
        else if (col.gameObject.tag == "Player")
        {
            respawning = false;
        }
    }
    
    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.name == "BoundAlert" && respawning == false)
        {
            iconRenderer.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        iconRenderer.enabled = false;
    }
}
