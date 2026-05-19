using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Scene Names")]
    public string mainSceneName = "SampleScene";
    public string simulatorSceneName = "CircuitSimulator";

    [Header("Transition Delay")]
    public float transitionDelay = 0.3f;

    private bool hasSavedMainScenePosition;
    private Vector3 savedMainScenePosition;
    private string savedMainSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persists between scenes
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ─── Called by NPC_Circuit after quest accepted ───────────────────
    public void LoadSimulator()
    {
        // Debug.Log("[SceneTransitionManager] LoadSimulator called!");
        StartCoroutine(LoadScene(simulatorSceneName));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        // Debug.Log($"[SceneTransitionManager] Loading scene: {sceneName}");
        yield return new WaitForSecondsRealtime(transitionDelay);
        Time.timeScale = 1f;

        if (TryGetSceneBuildIndex(sceneName, out int buildIndex))
        {
            SaveMainScenePositionIfLeaving(buildIndex);
            SceneManager.LoadScene(buildIndex);
            yield break;
        }

        Debug.LogError($"[SceneTransitionManager] Could not find scene '{sceneName}' in Build Settings.");
    }

    // ─── Called by Return button in simulator ─────────────────────────
    public void ReturnToMain()
    {
        StartCoroutine(LoadScene(mainSceneName));
    }

    private void SaveMainScenePositionIfLeaving(int destinationBuildIndex)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (!IsMainScene(activeScene.buildIndex))
            return;

        if (activeScene.buildIndex == destinationBuildIndex)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[SceneTransitionManager] Could not save player position because no Player-tagged object was found.");
            return;
        }

        InventoryController.Instance?.CacheCurrentInventory();

        savedMainScenePosition = player.transform.position;
        savedMainSceneName = activeScene.name;
        hasSavedMainScenePosition = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasSavedMainScenePosition)
            return;

        if (!IsMainScene(scene.buildIndex))
            return;

        StartCoroutine(RestorePlayerPositionAfterSceneStart());
    }

    private IEnumerator RestorePlayerPositionAfterSceneStart()
    {
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[SceneTransitionManager] Could not restore player position because no Player-tagged object was found.");
            yield break;
        }

        player.transform.position = savedMainScenePosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = savedMainScenePosition;
            rb.velocity = Vector2.zero;
        }

        Debug.Log($"[SceneTransitionManager] Restored player to {savedMainSceneName} position {savedMainScenePosition}.");
    }

    private bool IsMainScene(int buildIndex)
    {
        if (buildIndex < 0)
            return false;

        return TryGetSceneBuildIndex(mainSceneName, out int mainBuildIndex) &&
               buildIndex == mainBuildIndex;
    }

    private bool TryGetSceneBuildIndex(string sceneName, out int buildIndex)
    {
        buildIndex = SceneManager.GetSceneByName(sceneName).buildIndex;
        if (buildIndex >= 0)
            return true;

        string shortName = System.IO.Path.GetFileNameWithoutExtension(sceneName);
        string[] candidates =
        {
            sceneName,
            shortName,
            $"Assets/{sceneName}.unity",
            $"Assets/Scenes/{shortName}.unity"
        };

        foreach (string candidate in candidates)
        {
            buildIndex = SceneUtility.GetBuildIndexByScenePath(candidate);
            if (buildIndex >= 0)
                return true;
        }

        return false;
    }
}
