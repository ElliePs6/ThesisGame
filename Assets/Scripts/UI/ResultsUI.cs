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

        if (GameFlowManager.Instance.IsCounterbalancedFlow)
        {
            ShowCounterbalancedResults();
        }
        else
        {
            ShowLegacySingleModeResults();
        }
    }

    private void ShowCounterbalancedResults()
    {
        BlockResult block1 = GameFlowManager.Instance.Block1Results;
        BlockResult block2 = GameFlowManager.Instance.Block2Results;

        if (block1 == null || block2 == null)
        {
            resultsText.text =
                "No completed session found.\n\n" +
                "Play a session first to view results.";

            return;
        }

        string exportFolder = GetExportFolder();

        resultsText.text =
            "<b>Session Results</b>\n" +
            $"Participant: {GameFlowManager.Instance.ParticipantId} ({GameFlowManager.Instance.Username})\n\n" +

            BuildBlockSummary(block1) + "\n\n" +
            BuildBlockSummary(block2) + "\n\n" +

            "<b>Export Files Location</b>\n" +
            exportFolder;
    }

    private string BuildBlockSummary(BlockResult block)
    {
        string modeLabel = block.mode == AIMode.Adaptive
            ? "Adaptive CBR AI"
            : "Static Rule-Based AI";

        int totalActions = block.correctActions + block.wrongActions;

        return
            $"<b>Block {block.blockIndex} -- {modeLabel} (Subset {block.subsetGroup})</b>\n" +
            $"Final Influence: {block.finalInfluence}/100\n" +
            $"Correct Actions: {block.correctActions}/{totalActions}\n" +
            $"Success Rate: {block.successRate:P0}\n" +
            $"Average Reaction: {block.averageReactionTime:F1}s\n" +
            $"Action Variety: {block.actionVarietyScore:F2}\n" +
            $"Commit Actions: {block.commitCount}";
    }

    private void ShowLegacySingleModeResults()
    {
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
        GameFlowManager.Instance?.StartFirstBlock();
    }
}