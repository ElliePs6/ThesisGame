using System.IO;
using TMPro;
using UnityEngine;

public class ResultsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text resultsText;

    private void Start()
    {
        ShowResults();
    }

    private void ShowResults()
    {
        if (resultsText == null)
        {
            Debug.LogError(
                "ResultsUI: resultsText is not assigned in the Inspector.",
                this
            );

            return;
        }

        if (GameFlowManager.Instance == null)
        {
            resultsText.text =
                "No session results found.\n\n" +
                "Play a session first to view results.";

            return;
        }

        int correctActions = GameFlowManager.Instance.CorrectActions;
        int wrongActions = GameFlowManager.Instance.WrongActions;
        int totalActions = correctActions + wrongActions;

        if (totalActions == 0)
        {
            resultsText.text =
                "No completed session found.\n\n" +
                "Play a session first to view results.";

            return;
        }

        string modeLabel =
            GameFlowManager.Instance.SelectedMode == AIMode.Adaptive
                ? "Adaptive CBR AI"
                : "Static Rule-Based AI";

        string exportFolder = GetExportFolder();

        resultsText.text =
            "<b>Session Results</b>\n\n" +

            $"AI Mode: {modeLabel}\n" +
            $"Final Influence: {GameFlowManager.Instance.FinalInfluence}/100\n\n" +

            "<b>Performance</b>\n" +
            $"Correct Actions: {correctActions}/{totalActions}\n" +
            $"Success Rate: {GameFlowManager.Instance.SuccessRate:P0}\n" +
            $"Average Reaction: {GameFlowManager.Instance.AverageReactionTime:F1}s\n\n" +

            "<b>Behaviour</b>\n" +
            $"Action Variety: {GameFlowManager.Instance.ActionVarietyScore:F2}\n" +
            $"Commit Actions: {GameFlowManager.Instance.CommitCount}\n\n" +

            "<b>Export Files Location</b>\n" +
            exportFolder;
    }

    private string GetExportFolder()
    {
        string datasetPath = GameFlowManager.Instance.LastDatasetPath;
        string summaryPath = GameFlowManager.Instance.LastSummaryPath;

        string sourcePath = !string.IsNullOrWhiteSpace(datasetPath)
            ? datasetPath
            : summaryPath;

        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return "Export folder unavailable.";
        }

        string exportFolder = Path.GetDirectoryName(sourcePath);

        return string.IsNullOrWhiteSpace(exportFolder)
            ? "Export folder unavailable."
            : exportFolder;
    }

    public void OnBackClicked()
    {
        GameFlowManager.Instance?.GoToMainMenu();
    }

    public void OnPlayAgainClicked()
    {
        GameFlowManager.Instance?.GoToModeSelection();
    }
}