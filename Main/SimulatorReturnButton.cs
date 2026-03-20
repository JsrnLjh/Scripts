using UnityEngine;
using UnityEngine.UI;

public class SimulatorReturnButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnReturnClicked);
        else
            Debug.LogWarning("[SimulatorReturnButton] No Button component found!");
    }

    private void OnReturnClicked()
    {
        Debug.Log("[SimulatorReturnButton] Return clicked.");

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("[SimulatorReturnButton] SceneTransitionManager not found!");
            return;
        }

        SceneTransitionManager.Instance.ReturnToMain();
    }
}