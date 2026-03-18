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
        if (menuCanvas == null)
        {
            Debug.LogError("MenuCanvas is not assigned in MenuController.");
            return;
        }

        menuCanvas.SetActive(false);
        SetMobileControls(true);
    }

    public void ToggleMenu()
    {
        if (menuCanvas == null) return;

        if (menuCanvas.activeSelf) CloseMenu();
        else OpenMenu();
    }

    public void CloseMenu()
    {
        if (menuCanvas == null) return;

        menuCanvas.SetActive(false);
        SetMobileControls(true);
        PauseController.SetPause(false);
    }

    public void OpenMenu()
    {
        if (menuCanvas == null) return;

        menuCanvas.SetActive(true);
        SetMobileControls(false);
        PauseController.SetPause(true);
    }

    private void SetMobileControls(bool state)
    {
        if (floatingJoystick != null) floatingJoystick.SetActive(state);
        if (interactButton != null) interactButton.SetActive(state);
        if (menuButton != null) menuButton.SetActive(state);
        if (itemPopupContainer != null) itemPopupContainer.SetActive(state);
    }
}