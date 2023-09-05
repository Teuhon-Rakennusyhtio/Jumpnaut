using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelBreakPoint : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Barrel Killer"))
        {
            Destroy(gameObject);
        }
    }
}
