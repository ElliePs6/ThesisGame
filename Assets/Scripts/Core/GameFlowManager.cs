using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("XR")]
    [SerializeField] private GameObject xrSystemsPrefab;

    [Header("Scene Transition Fade")]
    [SerializeField] private float fadeDuration = 0.35f;

    public AIMode SelectedMode { get; private set; } = AIMode.Static;

    public string ParticipantId { get; private set; } = "01";
    public string Username { get; private set; } = "Anonymous";

    // ---------- Counterbalanced within-subjects study design ----------

    /// <summary>
    /// True όταν η τρέχουσα ροή είναι η αυτόματη, counterbalanced ακολουθία
    /// 2 blocks (η κανονική ροή για συμμετέχοντες στη μελέτη). False όταν
    /// χρησιμοποιείται η παλιά, χειροκίνητη StartSession(mode) -- π.χ. από
    /// το ModeSelection scene, που παραμένει μόνο ως εργαλείο dev/testing.
    /// </summary>
    public bool IsCounterbalancedFlow { get; private set; } = false;

    public int ConditionGroupIndex { get; private set; }
    public int CurrentBlockIndex { get; private set; } = 1;
    public RoundSubsetGroup CurrentBlockSubsetGroup { get; private set; } = RoundSubsetGroup.A;

    // Ποιο mode/subset παίζεται στο Block 1 ανά ομάδα συνθήκης (0-3).
    // Το Block 2 είναι πάντα το "αντίθετο" mode + το "αντίθετο" subset.
    private static readonly AIMode[] Block1ModeByGroup =
    {
        AIMode.Static,   // Group 0
        AIMode.Static,   // Group 1
        AIMode.Adaptive, // Group 2
        AIMode.Adaptive  // Group 3
    };

    private static readonly RoundSubsetGroup[] Block1SubsetByGroup =
    {
        RoundSubsetGroup.A, // Group 0
        RoundSubsetGroup.B, // Group 1
        RoundSubsetGroup.A, // Group 2
        RoundSubsetGroup.B  // Group 3
    };

    public BlockResult Block1Results { get; private set; }
    public BlockResult Block2Results { get; private set; }

    public int FinalInfluence { get; private set; }
    public int CorrectActions { get; private set; }
    public int WrongActions { get; private set; }
    public float AverageReactionTime { get; private set; }
    public float RiskSuccessRate { get; private set; }
    public float ActionVarietyScore { get; private set; }
    public float SuccessRate { get; private set; }
    public int CommitCount { get; private set; }
    public string LastDatasetPath { get; private set; }
    public string LastSummaryPath { get; private set; }

    private CanvasGroup fadeCanvasGroup;
    private Transform fadeCanvasTransform;
    private Camera cachedXRCamera;
    private bool isTransitioning = false;

    private const string PersistentSceneName = "DontDestroyOnLoad";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateFadeCanvas();
    }
    private void LateUpdate()
    {
        if (fadeCanvasTransform != null && fadeCanvasTransform.gameObject.activeSelf)
            PositionFadeCanvasInFrontOfCamera();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private IEnumerator Start()
    {
        yield return null;
        ConfigureSceneForVR();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ConfigureSceneAfterLoad());
    }

    private IEnumerator ConfigureSceneAfterLoad()
    {
        yield return null;
        ConfigureSceneForVR();
    }

    private void ConfigureSceneForVR()
    {
        EnsurePersistentXRSetup();
        EnsureEventSystem();
        TeleportPlayerToSpawnPoint();
    }

    // ---------- XR rig setup: guarantees exactly ONE persistent rig, ever ----------

    private void EnsurePersistentXRSetup()
    {
        var allOrigins = FindObjectsByType<XROrigin>(FindObjectsSortMode.None);

        if (allOrigins.Length == 0)
        {
            if (xrSystemsPrefab == null)
            {
                Debug.LogWarning("XR Systems prefab is not assigned on GameFlowManager.");
                return;
            }

            var rigInstance = Instantiate(xrSystemsPrefab);
            DontDestroyOnLoad(rigInstance);
            return;
        }

        if (allOrigins.Length == 1)
        {
            var singleRoot = allOrigins[0].transform.root;
            DontDestroyOnLoad(singleRoot.gameObject);
            return;
        }

        // More than one XR Origin exists at the same time. This happens when
        // a scene has its own baked-in "XR Systems New" (needed the very
        // first time this scene ever loads, to bootstrap the persistent rig)
        // AND a persistent rig from a previous scene already exists.
        // Keep whichever one is already parented under DontDestroyOnLoad,
        // and destroy every other duplicate rig entirely -- otherwise we end
        // up with two permanently-persistent rigs (duplicate controllers,
        // duplicate EventSystems, and the player getting teleported using
        // the wrong / freshly-loaded rig's default facing direction).
        Transform keepRoot = null;

        foreach (var origin in allOrigins)
        {
            var root = origin.transform.root;
            if (root.gameObject.scene.name == PersistentSceneName)
            {
                keepRoot = root;
                break;
            }
        }

        if (keepRoot == null)
        {
            keepRoot = allOrigins[0].transform.root;
        }

        foreach (var origin in allOrigins)
        {
            var root = origin.transform.root;
            if (root != keepRoot)
            {
                Destroy(root.gameObject);
            }
        }

        DontDestroyOnLoad(keepRoot.gameObject);
    }

    private void EnsureEventSystem()
    {
        var allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        if (allEventSystems.Length == 0)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<XRUIInputModule>();
            DontDestroyOnLoad(eventSystemObject);
            return;
        }

        if (allEventSystems.Length == 1)
        {
            var single = allEventSystems[0];

            if (single.GetComponent<XRUIInputModule>() == null)
            {
                single.gameObject.AddComponent<XRUIInputModule>();
            }

            if (single.transform.parent == null)
            {
                DontDestroyOnLoad(single.gameObject);
            }

            return;
        }

        // Περισσότερα από ένα EventSystem ταυτόχρονα -- κρατάμε όποιο είναι ήδη
        // persistent (μέσα στη σκηνή "DontDestroyOnLoad") και καταστρέφουμε
        // κάθε άλλο duplicate.
        EventSystem keep = null;

        foreach (var es in allEventSystems)
        {
            if (es.gameObject.scene.name == PersistentSceneName)
            {
                keep = es;
                break;
            }
        }

        if (keep == null)
        {
            keep = allEventSystems[0];
        }

        foreach (var es in allEventSystems)
        {
            if (es != keep)
            {
                Destroy(es.gameObject);
            }
        }

        if (keep.GetComponent<XRUIInputModule>() == null)
        {
            keep.gameObject.AddComponent<XRUIInputModule>();
        }

        if (keep.transform.parent == null)
        {
            DontDestroyOnLoad(keep.gameObject);
        }
    }

    private void TeleportPlayerToSpawnPoint()
    {
        var spawnPoint = FindFirstObjectByType<PlayerSpawnPoint>();
        var xrOrigin = FindFirstObjectByType<XROrigin>();

        if (spawnPoint == null || xrOrigin == null || xrOrigin.Camera == null)
            return;

        var cameraTransform = xrOrigin.Camera.transform;
        var originTransform = xrOrigin.transform;

        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;

        Vector3 targetForward = spawnPoint.transform.forward;
        targetForward.y = 0f;

        if (cameraForward.sqrMagnitude > 0.0001f && targetForward.sqrMagnitude > 0.0001f)
        {
            float angle = Vector3.SignedAngle(cameraForward, targetForward, Vector3.up);
            originTransform.RotateAround(cameraTransform.position, Vector3.up, angle);
        }

        // Μόνο οριζόντια μετακίνηση (X/Z) -- ΔΕΝ πειράζουμε το ύψος (Y),
        // αφού το headset (Floor tracking) υπολογίζει μόνο του το σωστό
        // ύψος κάμερας πάνω από το πραγματικό πάτωμα.
        Vector3 positionDelta = spawnPoint.transform.position - cameraTransform.position;
        positionDelta.y = 0f;
        originTransform.position += positionDelta;
    }

    // ---------- Fade canvas setup (World Space, so it is visible inside the VR headset) ----------

    private void CreateFadeCanvas()
    {
        var canvasObject = new GameObject("SceneFadeCanvas");
        DontDestroyOnLoad(canvasObject);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 999;

        var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.dynamicPixelsPerUnit = 1;

        canvasObject.AddComponent<GraphicRaycaster>();

        var rectTransform = canvasObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(8f, 8f);

        var imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);

        var image = imageObject.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        var imageRect = image.rectTransform;
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        fadeCanvasGroup = canvasObject.AddComponent<CanvasGroup>();

        // Ξεκινάμε ήδη ΜΑΥΡΟ και ενεργό -- έτσι δεν προλαβαίνει να φανεί
        // καθόλου το ακατέργαστο skybox/σκηνή στα πρώτα frames, πριν προλάβει
        // να τρέξει το πρώτο GoToMainMenu() fade. Το πρώτο Fade(1f) μέσα στο
        // FadeAndLoadScene θα είναι απλά ένα no-op (1 -> 1).
        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = false;

        fadeCanvasTransform = canvasObject.transform;
        canvasObject.SetActive(true);

        PositionFadeCanvasInFrontOfCamera();
    }

    private void PositionFadeCanvasInFrontOfCamera()
    {
        if (cachedXRCamera == null)
        {
            cachedXRCamera = Camera.main;
        }

        if (cachedXRCamera == null || fadeCanvasTransform == null)
            return;

        var camTransform = cachedXRCamera.transform;
        fadeCanvasTransform.position = camTransform.position + camTransform.forward * 0.15f;
        fadeCanvasTransform.rotation = camTransform.rotation;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.gameObject.SetActive(true);
        PositionFadeCanvasInFrontOfCamera();

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0.5f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);

            PositionFadeCanvasInFrontOfCamera();

            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        if (targetAlpha <= 0.01f)
        {
            fadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (isTransitioning)
            yield break;

        isTransitioning = true;

        yield return StartCoroutine(Fade(1f));

        var loadOperation = SceneManager.LoadSceneAsync(sceneName);
        while (loadOperation != null && !loadOperation.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(Fade(0f));

        isTransitioning = false;
    }

    // ---------- Public scene navigation (fades) ----------

    public void GoToMainMenu()
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeAndLoadScene("MainMenuNew"));
        }
    }

    public void GoToModeSelection()
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeAndLoadScene("ModeSelection"));
        }
    }

    public void StartSession(AIMode mode)
    {
        // Dev/testing shortcut μόνο (μέσω ModeSelection scene) -- δεν
        // ανήκει στην κανονική, counterbalanced ροή της μελέτης.
        IsCounterbalancedFlow = false;
        SelectedMode = mode;

        if (!isTransitioning)
        {
            StartCoroutine(FadeAndLoadScene("OfficeSession"));
        }
    }

    public void GoToResults()
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeAndLoadScene("Results"));
        }
    }

    public void StoreSessionResults(
        int finalInfluence,
        int correctActions,
        int wrongActions,
        float successRate,
        float averageReactionTime,
        float riskSuccessRate,
        float actionVarietyScore,
        int commitCount
    )
    {
        FinalInfluence = finalInfluence;
        CorrectActions = correctActions;
        WrongActions = wrongActions;
        SuccessRate = successRate;
        AverageReactionTime = averageReactionTime;
        RiskSuccessRate = riskSuccessRate;
        ActionVarietyScore = actionVarietyScore;
        CommitCount = commitCount;
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void StoreDatasetPath(string datasetPath)
    {
        LastDatasetPath = datasetPath;
    }

    public void StoreSummaryPath(string summaryPath)
    {
        LastSummaryPath = summaryPath;
    }

    public void SetParticipant(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            username = "Anonymous";
        }

        Username = username.Trim();

        int nextId = PlayerPrefs.GetInt("NextParticipantId", 1);
        ParticipantId = nextId.ToString("00");

        PlayerPrefs.SetInt("NextParticipantId", nextId + 1);

        // Κυκλική ανάθεση σε 4 ομάδες συνθήκης (0-3), ίδιο pattern με το
        // ParticipantId -- εξασφαλίζει ισόρροπη κατανομή participants στις
        // 4 ομάδες του πλήρους 2x2 counterbalancing (σειρά mode x subset).
        int nextCondition = PlayerPrefs.GetInt("NextConditionGroup", 0);
        ConditionGroupIndex = nextCondition % 4;
        PlayerPrefs.SetInt("NextConditionGroup", nextCondition + 1);

        PlayerPrefs.Save();
    }

    // ---------- Counterbalanced 2-block flow (κανονική ροή μελέτης) ----------

    public void StartFirstBlock()
    {
        IsCounterbalancedFlow = true;
        ConfigureBlock(1);

        if (!isTransitioning)
        {
            StartCoroutine(FadeAndLoadScene("OfficeSession"));
        }
    }

    /// <summary>
    /// Καλείται από το GameManager όταν τελειώσει ένα block (10/10 rounds).
    /// Αν μόλις τελείωσε το Block 1, ξεκινάει αυτόματα το Block 2 (άλλο mode,
    /// άλλο subset rounds). Αν μόλις τελείωσε το Block 2, πάει στο Results.
    /// </summary>
    public void OnBlockCompleted()
    {
        if (CurrentBlockIndex == 1)
        {
            ConfigureBlock(2);

            if (!isTransitioning)
            {
                StartCoroutine(FadeAndLoadScene("OfficeSession"));
            }
        }
        else
        {
            GoToResults();
        }
    }

    private void ConfigureBlock(int blockIndex)
    {
        CurrentBlockIndex = blockIndex;

        AIMode block1Mode = Block1ModeByGroup[ConditionGroupIndex];
        RoundSubsetGroup block1Subset = Block1SubsetByGroup[ConditionGroupIndex];

        if (blockIndex == 1)
        {
            SelectedMode = block1Mode;
            CurrentBlockSubsetGroup = block1Subset;
        }
        else
        {
            SelectedMode = block1Mode == AIMode.Static ? AIMode.Adaptive : AIMode.Static;
            CurrentBlockSubsetGroup = block1Subset == RoundSubsetGroup.A
                ? RoundSubsetGroup.B
                : RoundSubsetGroup.A;
        }
    }

    public void StoreBlockResults(
        int blockIndex,
        AIMode mode,
        RoundSubsetGroup subsetGroup,
        int finalInfluence,
        int correctActions,
        int wrongActions,
        float successRate,
        float averageReactionTime,
        float riskSuccessRate,
        float actionVarietyScore,
        int commitCount
    )
    {
        var result = new BlockResult
        {
            blockIndex = blockIndex,
            mode = mode,
            subsetGroup = subsetGroup,
            finalInfluence = finalInfluence,
            correctActions = correctActions,
            wrongActions = wrongActions,
            successRate = successRate,
            averageReactionTime = averageReactionTime,
            riskSuccessRate = riskSuccessRate,
            actionVarietyScore = actionVarietyScore,
            commitCount = commitCount
        };

        if (blockIndex == 1)
        {
            Block1Results = result;
        }
        else
        {
            Block2Results = result;
        }
    }
}

[System.Serializable]
public class BlockResult
{
    public int blockIndex;
    public AIMode mode;
    public RoundSubsetGroup subsetGroup;
    public int finalInfluence;
    public int correctActions;
    public int wrongActions;
    public float successRate;
    public float averageReactionTime;
    public float riskSuccessRate;
    public float actionVarietyScore;
    public int commitCount;
}