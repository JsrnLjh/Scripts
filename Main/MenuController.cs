using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuCanvas;

    [Header("Mobile Controls")]
    public GameObject floatingJoystick;
    public GameObject interactButton;
    public GameObject menuButton;
    public GameObject itemPopupContainer; 

    private void Start()
    {
        menuCanvas.SetActive(false);
        SetMobileControls(true);
    }

    public void ToggleMenu()
    {
        bool isMenuOpening = !menuCanvas.activeSelf;
        menuCanvas.SetActive(isMenuOpening);
        SetMobileControls(!isMenuOpening);
    }

    public void CloseMenu()
    {
        menuCanvas.SetActive(false);
        SetMobileControls(true);
    }

    public void OpenMenu()
    {
        menuCanvas.SetActive(true);
        SetMobileControls(false);
    }

    private void SetMobileControls(bool state)
    {
        if (floatingJoystick != null) floatingJoystick.SetActive(state);
        if (interactButton != null) interactButton.SetActive(state);
        if (menuButton != null) menuButton.SetActive(state);
        if (itemPopupContainer != null) itemPopupContainer.SetActive(state); 
    }
}