using TMPro;
using UnityEngine;

public class ResultsUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text resultsText;

    private void Start()
    {
        ShowResults();
    }

    private void ShowResults()
    {
        if (resultsText == null)
        {
            Debug.LogError("ResultsUI: resultsText is not assigned in the Inspector.");
            return;
        }

        if (GameFlowManager.Instance == null)
        {
            resultsText.text = "No session results found.\n\nPlay a session first to view results.";
            return;
        }

        string modeLabel = GameFlowManager.Instance.SelectedMode == AIMode.Adaptive
            ? "Adaptive CBR AI"
            : "Static Rule-Based AI";

        int totalActions = GameFlowManager.Instance.CorrectActions + GameFlowManager.Instance.WrongActions;

        if (totalActions == 0)
        {
            resultsText.text =
                "No completed session found.\n\n" +
                "Play a session first to view results.";
            return;
        }

        resultsText.text =
            "Current Session Results\n\n" +

            $"Selected AI Mode: {modeLabel}\n" +
            $"Final Influence Score: {GameFlowManager.Instance.FinalInfluence} / 100\n\n" +

            "Performance Metrics\n" +
            $"Correct Actions: {GameFlowManager.Instance.CorrectActions} / {totalActions}\n" +
            $"Wrong Actions: {GameFlowManager.Instance.WrongActions} / {totalActions}\n" +
            $"Success Rate: {GameFlowManager.Instance.SuccessRate:P0}\n\n" +

            "Behaviour Metrics\n" +
            $"Average Reaction Time: {GameFlowManager.Instance.AverageReactionTime:F2}s\n" +
            $"Risk Success Rate: {GameFlowManager.Instance.RiskSuccessRate:F2}\n" +
            $"Action Variety Score: {GameFlowManager.Instance.ActionVarietyScore:F2}\n" +
            $"Commit Count: {GameFlowManager.Instance.CommitCount}" +
            "Dataset Export\n" +
            $"CSV Path: {GameFlowManager.Instance.LastDatasetPath}" +
            "Dataset Export\n" +
            $"Round Dataset CSV: {GameFlowManager.Instance.LastDatasetPath}\n" +
            $"Session Summary CSV: {GameFlowManager.Instance.LastSummaryPath}";
    }

    public void OnBackClicked()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToMainMenu();
        }
    }
    public void OnPlayAgainClicked()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToModeSelection();
        }
    }
}