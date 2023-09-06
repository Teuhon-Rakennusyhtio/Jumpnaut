using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILadderInteractable
{
    public void OnLadderEnter(float xCoord);
    public void OnLadderExit();
}
