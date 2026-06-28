using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public AIMode SelectedMode { get; private set; } = AIMode.Static;

    public string ParticipantId { get; private set; } = "01";
    public string Username { get; private set; } = "Anonymous";

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
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToModeSelection()
    {
        SceneManager.LoadScene("ModeSelection");
    }

    public void StartSession(AIMode mode)
    {
        SelectedMode = mode;
        SceneManager.LoadScene("OfficeSession");
    }

    public void GoToResults()
    {
        SceneManager.LoadScene("Results");
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
        PlayerPrefs.Save();
    }
}