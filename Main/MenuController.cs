using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject floatingJoystick;
    public GameObject interactButton;
    public GameObject menuButton;

    void Start()
    {
        menuCanvas.SetActive(false);

        if (floatingJoystick != null) floatingJoystick.SetActive(true);
        if (interactButton != null) interactButton.SetActive(true);
        if (menuButton != null) menuButton.SetActive(true);
    }


    public void ToggleMenu()
    {
        // Toggle menu button on/off
        bool isMenuOpening = !menuCanvas.activeSelf;
        menuCanvas.SetActive(isMenuOpening);

        // Hide/Show mobile controls based on menu state
        if (floatingJoystick != null) floatingJoystick.SetActive(!isMenuOpening);
        if (interactButton != null) interactButton.SetActive(!isMenuOpening);
        if (menuButton != null) menuButton.SetActive(!isMenuOpening);
    }
}
