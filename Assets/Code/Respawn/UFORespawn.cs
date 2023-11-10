using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFORespawn : MonoBehaviour
{
    public SpriteRenderer ufoRenderer;
    
    void Start()
    {
        ufoRenderer = GetComponent<SpriteRenderer>();
        ufoRenderer.enabled = false;
    }

    public void UfoActive()
    {
        ufoRenderer.enabled = true;
    }

    public void UfoInactive()
    {
        ufoRenderer.enabled = false;
    }
}
