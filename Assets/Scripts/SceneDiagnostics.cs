using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

public class SceneDiagnostics : MonoBehaviour
{
    [Header("Πότε να τρέξει")]
    [SerializeField] private bool logOnStart = true;
    [SerializeField] private bool logOnEverySceneLoad = true;

    private void Start()
    {
        if (logOnStart)
        {
            LogEverything("START");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (logOnEverySceneLoad)
        {
            LogEverything($"SCENE LOADED: {scene.name}");
        }
    }

    [ContextMenu("Log Now")]
    public void LogNow()
    {
        LogEverything("MANUAL TRIGGER");
    }

    private void LogEverything(string context)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine($"[SceneDiagnostics] {context} @ {Time.time:F2}s");
        sb.AppendLine("========================================");

        // 1. Πόσες σκηνές είναι φορτωμένες ταυτόχρονα
        sb.AppendLine($"Loaded Scene Count: {SceneManager.sceneCount}");
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            sb.AppendLine($"  - Scene[{i}]: {s.name} | isLoaded={s.isLoaded} | rootCount={s.rootCount}");
        }

        // 2. Duplicates ελέγχων σε κρίσιμα singletons
        var gameFlowManagers = FindObjectsByType<GameFlowManager>(FindObjectsSortMode.None);
        sb.AppendLine($"GameFlowManager count: {gameFlowManagers.Length}");
        foreach (var g in gameFlowManagers)
        {
            sb.AppendLine($"  - {GetFullPath(g.transform)}");
        }

        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        sb.AppendLine($"EventSystem count: {eventSystems.Length}");
        foreach (var e in eventSystems)
        {
            sb.AppendLine($"  - {GetFullPath(e.transform)} (enabled={e.enabled})");
        }

        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        sb.AppendLine($"Canvas count: {canvases.Length}");
        foreach (var c in canvases)
        {
            sb.AppendLine($"  - {GetFullPath(c.transform)} (active={c.gameObject.activeInHierarchy}, renderMode={c.renderMode})");
        }

        var xrOrigins = FindObjectsByType<XROrigin>(FindObjectsSortMode.None);
        sb.AppendLine($"XROrigin count: {xrOrigins.Length}");
        foreach (var x in xrOrigins)
        {
            sb.AppendLine($"  - {GetFullPath(x.transform)}");
        }

        // 3. Πλήρες Hierarchy dump (όλα τα root objects κάθε σκηνής + DontDestroyOnLoad)
        sb.AppendLine("---- FULL HIERARCHY ----");
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            sb.AppendLine($"[Scene: {s.name}]");
            foreach (var root in s.GetRootGameObjects())
            {
                DumpHierarchy(root.transform, sb, 1);
            }
        }

        sb.AppendLine("========================================");

        Debug.Log(sb.ToString());
    }

    private void DumpHierarchy(Transform t, StringBuilder sb, int depth)
    {
        sb.AppendLine($"{new string(' ', depth * 2)}- {t.name} (active={t.gameObject.activeSelf})");
        foreach (Transform child in t)
        {
            DumpHierarchy(child, sb, depth + 1);
        }
    }

    private string GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}