using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Scene Names")]
    public string mainSceneName = "Scenes/SampleScene";
    public string simulatorSceneName = "Scenes/CircuitSimulator";

    [Header("Transition Delay")]
    public float transitionDelay = 0.3f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persists between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ─── Called by NPC_Circuit after quest accepted ───────────────────
    public void LoadSimulator()
    {
        Debug.Log("[SceneTransitionManager] LoadSimulator called!");
        StartCoroutine(LoadScene(simulatorSceneName));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        Debug.Log($"[SceneTransitionManager] Loading scene: {sceneName}");
        yield return new WaitForSecondsRealtime(transitionDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // ─── Called by Return button in simulator ─────────────────────────
    public void ReturnToMain()
    {
        StartCoroutine(LoadScene(mainSceneName));
    }
}