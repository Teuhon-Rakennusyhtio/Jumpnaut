using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public int playerDead = 0;
    public bool AllAreDead = false;

    public void DeathCount()
    {
        playerDead += 1;
    }

    public void DeathReducer()
    {
        playerDead -= 1;
    }

    public bool DeathToll(float deathLimit)
    {
        if (playerDead == deathLimit)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}