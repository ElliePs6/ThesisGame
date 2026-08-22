using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent (DontDestroyOnLoad) ήχος υποβάθρου.
/// - Παίζει ένα "chill" (piano) κομμάτι από την αρχή του παιχνιδιού (MainMenuNew).
/// - Παίζει, σε loop και εξ αρχής αλλά με ένταση 0, τον ήχο ambience (πουλιά/αμάξια).
/// - Όταν φορτώνει η σκηνή "OfficeSession", κάνει crossfade: το chill σβήνει τελείως
///   (0) και το ambience ανεβαίνει -- ακούγονται μόνο οι ήχοι της πόλης.
/// - Όταν φορτώνει η σκηνή "Results" (τέλος session), κάνει το αντίστροφο crossfade:
///   το chill ξαναγυρνάει στην αρχική του ένταση, το ambience σβήνει.
/// </summary>
public class AmbientAudioManager : MonoBehaviour
{
    public static AmbientAudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource που παίζει το relaxing piano clip.")]
    [SerializeField] private AudioSource chillSource;

    [Tooltip("AudioSource που παίζει το city ambience clip (πουλιά/αμάξια).")]
    [SerializeField] private AudioSource ambienceSource;

    [Header("Clips")]
    [SerializeField] private AudioClip chillClip;
    [SerializeField] private AudioClip ambienceClip;

    [Header("Volumes")]
    [Tooltip("Ένταση του chill πριν/μετά το session -- στο MainMenu και στο Results (0-1).")]
    [Range(0f, 1f)] [SerializeField] private float chillStartVolume = 0.8f;

    [Tooltip("Ένταση του chill μέσα στο OfficeSession -- 0 σημαίνει πλήρες σβήσιμο, ακούγονται μόνο οι ήχοι πόλης.")]
    [Range(0f, 1f)] [SerializeField] private float chillOfficeVolume = 0f;

    [Tooltip("Ένταση του ambience μέσα στο OfficeSession.")]
    [Range(0f, 1f)] [SerializeField] private float ambienceOfficeVolume = 0.6f;

    [Header("Crossfade Timing")]
    [SerializeField] private float crossfadeDuration = 3f;

    [Header("Scene Triggers")]
    [Tooltip("Το όνομα της σκηνής που πυροδοτεί το crossfade προς το city ambience.")]
    [SerializeField] private string officeSceneName = "OfficeSession";

    [Tooltip("Το όνομα της σκηνής που πυροδοτεί την επιστροφή στο chill (τέλος session).")]
    [SerializeField] private string resultsSceneName = "Results";

    private Coroutine crossfadeRoutine;

    private bool isChillMuted = false;
    private bool isAmbienceMuted = false;

    public bool IsChillMuted => isChillMuted;
    public bool IsAmbienceMuted => isAmbienceMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupSources();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SetupSources()
    {
        if (chillSource == null || ambienceSource == null)
        {
            Debug.LogWarning("AmbientAudioManager: AudioSources δεν έχουν οριστεί στο Inspector.");
            return;
        }

        chillSource.clip = chillClip;
        chillSource.loop = true;
        chillSource.playOnAwake = false;
        chillSource.volume = chillStartVolume;

        ambienceSource.clip = ambienceClip;
        ambienceSource.loop = true;
        ambienceSource.playOnAwake = false;
        ambienceSource.volume = 0f;

        if (chillClip != null)
        {
            chillSource.Play();
        }

        // Το ambience παίζει από την αρχή, αλλά σιωπηλό -- έτσι όταν κάνουμε
        // fade in δεν υπάρχει καθυστέρηση/κόψιμο στην εκκίνηση του clip.
        if (ambienceClip != null)
        {
            ambienceSource.Play();
        }
    }

    /// <summary>
    /// Ενεργοποιεί/απενεργοποιεί (mute) το chill (piano) κομμάτι, ανεξάρτητα από το ambience.
    /// Χρησιμοποιεί AudioSource.mute -- έτσι το τρέχον volume (πριν/μετά το crossfade)
    /// παραμένει αναλλοίωτο, απλά δεν ακούγεται όσο είναι muted.
    /// </summary>
    public void ToggleChillMute()
    {
        if (chillSource == null)
            return;

        isChillMuted = !isChillMuted;
        chillSource.mute = isChillMuted;
    }

    /// <summary>
    /// Ενεργοποιεί/απενεργοποιεί (mute) το city ambience (πουλιά/αμάξια), ανεξάρτητα από το chill.
    /// </summary>
    public void ToggleAmbienceMute()
    {
        if (ambienceSource == null)
            return;

        isAmbienceMuted = !isAmbienceMuted;
        ambienceSource.mute = isAmbienceMuted;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == officeSceneName)
        {
            TriggerOfficeCrossfade();
        }
        else if (scene.name == resultsSceneName)
        {
            TriggerResultsCrossfade();
        }
    }

    /// <summary>
    /// Ξεκινάει το crossfade προς το OfficeSession: chill -> 0 (πλήρες σβήσιμο),
    /// ambience -> ambienceOfficeVolume. Μπορεί επίσης να κληθεί χειροκίνητα
    /// αν χρειαστεί (π.χ. testing).
    /// </summary>
    public void TriggerOfficeCrossfade()
    {
        StartCrossfade(chillOfficeVolume, ambienceOfficeVolume);
    }

    /// <summary>
    /// Ξεκινάει το αντίστροφο crossfade (τέλος session, οθόνη Results):
    /// chill -> πίσω στην αρχική ένταση, ambience -> 0.
    /// </summary>
    public void TriggerResultsCrossfade()
    {
        StartCrossfade(chillStartVolume, 0f);
    }

    private void StartCrossfade(float targetChillVolume, float targetAmbienceVolume)
    {
        if (chillSource == null || ambienceSource == null)
            return;

        if (crossfadeRoutine != null)
        {
            StopCoroutine(crossfadeRoutine);
        }

        crossfadeRoutine = StartCoroutine(CrossfadeVolumes(targetChillVolume, targetAmbienceVolume));
    }

    private IEnumerator CrossfadeVolumes(float targetChillVolume, float targetAmbienceVolume)
    {
        float elapsed = 0f;
        float startChillVolume = chillSource.volume;
        float startAmbienceVolume = ambienceSource.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crossfadeDuration;

            chillSource.volume = Mathf.Lerp(startChillVolume, targetChillVolume, t);
            ambienceSource.volume = Mathf.Lerp(startAmbienceVolume, targetAmbienceVolume, t);

            yield return null;
        }

        chillSource.volume = targetChillVolume;
        ambienceSource.volume = targetAmbienceVolume;
    }
}