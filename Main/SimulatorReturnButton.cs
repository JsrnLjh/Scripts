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

        // Auto-create SceneTransitionManager if it doesn't exist
        // This handles the case where CircuitSimulator is loaded directly
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning("[SimulatorReturnButton] SceneTransitionManager not found " +
                             "— creating one automatically.");

            GameObject stm = new GameObject("SceneTransitionManager");
            stm.AddComponent<SceneTransitionManager>();
        }

        SceneTransitionManager.Instance.ReturnToMain();
    }
}