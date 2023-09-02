using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSubMenu : MonoBehaviour
{
    public void MenuIn()
    {
        gameObject.GetComponentInParent<MainMenu>().StartScreenReturnAnimation();
    }
}
