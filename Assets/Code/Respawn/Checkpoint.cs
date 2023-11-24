using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    GameObject[] enemies;
    GameObject[] platforms;

    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        platforms = GameObject.FindGameObjectsWithTag("MovingPlatform");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Player")
        {
            DespawnEnemies();
            DespawnMovingPlatforms();
        }
    }

    void DespawnEnemies()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (this.transform.position.y > enemies[i].transform.position.y + 15f)
            {
                enemies[i].SetActive(false);
            }
        }
    }

    void DespawnMovingPlatforms()
    {
        for (int i = 0; i < platforms.Length; i++)
        {
            if (this.transform.position.y > platforms[i].transform.position.y + 15f)
            {
                platforms[i].SetActive(false);
            }
        }
    }
}
