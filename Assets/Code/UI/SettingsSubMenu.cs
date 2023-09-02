using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SettingsSubMenu : MonoBehaviour
{
    [SerializeField] GameObject _doneButton;
    public void MenuIn()
    {
        //gameObject.GetComponentInParent<MainMenu>().SettingsScreenEnterAnimation();
        _doneButton.GetComponent<Button>().interactable = true;
        EventSystem.current.SetSelectedGameObject(_doneButton);
    }
}
