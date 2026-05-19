using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuCanvas;

    [Header("Mobile Controls")]
    public GameObject floatingJoystick;
    public GameObject interactButton;
    public GameObject menuButton;
    public GameObject itemPopupContainer;

    private Button closeButton;

    private void Awake()
    {
        AutoAssignReferences();
        BindCloseButton();
    }

    private void Start()
    {
        AutoAssignReferences();
        BindCloseButton();

        if (menuCanvas == null)
        {
            // Debug.LogError("MenuCanvas is not assigned in MenuController.");
            return;
        }

        menuCanvas.SetActive(false);
        SetMobileControls(true);
    }

    public void ToggleMenu()
    {
        AutoAssignReferences();
        if (menuCanvas == null) return;

        if (menuCanvas.activeSelf) CloseMenu();
        else OpenMenu();
    }

    public void CloseMenu()
    {
        AutoAssignReferences();
        if (menuCanvas == null) return;

        menuCanvas.SetActive(false);
        SetMobileControls(true);
        PauseController.SetPause(false);
    }

    public void OpenMenu()
    {
        AutoAssignReferences();
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

    private void AutoAssignReferences()
    {
        if (menuCanvas == null)
            menuCanvas = FindSceneObject("MenuCanvas");

        if (floatingJoystick == null)
            floatingJoystick = FindSceneObject("Floating Joystick");

        if (interactButton == null)
            interactButton = FindSceneObject("InteractButton");

        if (menuButton == null)
            menuButton = FindSceneObject("MenuButton");

        if (itemPopupContainer == null)
            itemPopupContainer = FindSceneObject("ItemPopupContainer");
    }

    private void BindCloseButton()
    {
        if (menuCanvas == null)
            return;

        closeButton = FindCloseButton(menuCanvas.transform);
        if (closeButton == null)
            return;

        closeButton.onClick.RemoveListener(CloseMenu);
        closeButton.onClick.AddListener(CloseMenu);
    }

    private Button FindCloseButton(Transform parent)
    {
        Button[] buttons = parent.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button != null && button.name == "Close")
                return button;
        }

        return null;
    }

    private GameObject FindSceneObject(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform == null || !transform.gameObject.scene.IsValid())
                continue;

            if (transform.name == objectName)
                return transform.gameObject;
        }

        return null;
    }
}
