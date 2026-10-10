using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using TMPro;

public class GameLogger : MonoBehaviour
{
    [Header("Optional UI")]
    public TMP_Text logText;

    [Header("Session Metadata")]
    public string participantId = "01";
    public string username = "Anonymous";
    public string sessionId;

    private List<string> entries = new List<string>();
    private List<SessionRoundRecord> roundRecords = new List<SessionRoundRecord>();

    private void Awake()
    {
        if (GameFlowManager.Instance != null)
        {
            participantId = GameFlowManager.Instance.ParticipantId;
            username = GameFlowManager.Instance.Username;
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
            Log($"SESSION STARTED | ParticipantID={participantId} | Username={username} | SessionID={sessionId}");
        }
    }

    public void Log(string message)
    {
        string entry = $"[{Time.time:F1}s] {message}";
        entries.Add(entry);

        Debug.Log(entry);

        if (logText != null)
        {
            logText.text = string.Join("\n", entries);
        }
    }

    public List<string> GetEntries()
    {
        return entries;
    }

    public void LogRoundResult(
        int roundNumber,
        string scenarioId,
        string aiMode,
        string statement,
        string selectedAction,
        string preferredAction,
        bool isCorrect,
        string outcomeQuality,
        string nextNpcState,
        float reactionTime,
        int baseInfluenceChange,
        int finalInfluenceChange,
        int adaptiveAdjustment,
        int influenceTotal,
        string authorityFeedback,
        string aiResponse,
        string matchedCaseId,
        float matchScore,
        string adaptiveReason,
        string behaviourLabel,
        bool wasAlreadyResolved,
        float ambiguityGap,      
        string secondCaseId,  
        PlayerBehaviourTracker tracker,
        string caseType,
        string difficulty,
        string commitTiming,
        string roundPosition,
        int conditionGroupIndex = -1,
        int blockIndex = -1,
        string subsetGroup = "N/A")
    {
        roundRecords.Add(new SessionRoundRecord
        {
            sessionId = sessionId,
            participantId = participantId,
            username = username,
            roundNumber = roundNumber,
            scenarioId = scenarioId,
            aiMode = aiMode,
            statement = statement,
            selectedAction = selectedAction,
            preferredAction = preferredAction,
            isCorrect = isCorrect,
            outcomeQuality = outcomeQuality,
            nextNpcState = nextNpcState,
            reactionTime = reactionTime,
            baseInfluenceChange = baseInfluenceChange,
            finalInfluenceChange = finalInfluenceChange,
            adaptiveAdjustment = adaptiveAdjustment,
            influenceTotal = influenceTotal,
            authorityFeedback = authorityFeedback,
            aiResponse = aiResponse,
            matchedCaseId = matchedCaseId,
            matchScore = matchScore,
            adaptiveReason = adaptiveReason,
            behaviourLabel = behaviourLabel,
            wasAlreadyResolved = wasAlreadyResolved,
            ambiguityGap = ambiguityGap,
            secondCaseId = secondCaseId,
            caseType = caseType,
            difficulty = difficulty,
            commitTiming = commitTiming,
            roundPosition = roundPosition,
            conditionGroupIndex = conditionGroupIndex,
            blockIndex = blockIndex,
            subsetGroup = subsetGroup,
            totalActions = tracker != null ? tracker.totalActions : 0,
            successRate = tracker != null ? tracker.GetSuccessRate() : 0f,
            riskPreference = tracker != null ? tracker.GetRiskPreference() : 0f,
            averageReactionTime = tracker != null ? tracker.averageReactionTime : 0f,

            commitCount = tracker != null ? tracker.commitCount : 0,
            successfulCommitCount = tracker != null ? tracker.successfulCommitCount : 0,
            riskSuccessRate = tracker != null ? tracker.GetRiskSuccessRate() : 0f,
            actionVarietyScore = tracker != null ? tracker.GetActionVarietyScore() : 0f,
            bestCount = tracker != null ? tracker.bestCount : 0,
            acceptableCount = tracker != null ? tracker.acceptableCount : 0,
            badCount = tracker != null ? tracker.badCount : 0,
            veryBadCount = tracker != null ? tracker.veryBadCount : 0
        });
    }

    public string ExportSessionDataset()
    {
        string fileName = $"session_dataset_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        string csv = BuildCsv();
        File.WriteAllText(path, csv, Encoding.UTF8);
        AndroidPublicStorage.SaveToDownloads(fileName, csv);   // <-- πρόσθεσε αυτό
        Log($"DATASET EXPORTED | {path}");
        return path;
    }

    public string ExportSessionSummary(
        string aiMode,
        int finalInfluence,
        PlayerBehaviourTracker tracker,
        int conditionGroupIndex = -1,
        int blockIndex = -1,
        string subsetGroup = "N/A"
    )
    {
        string fileName = $"session_summary_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        StringBuilder builder = new StringBuilder();

        builder.AppendLine(
            "sessionId,playerId,username,aiMode,conditionGroupIndex,blockIndex,subsetGroup,finalInfluence,totalActions,correctActions,wrongActions,successRate,averageReactionTime,riskPreference,commitCount,successfulCommitCount,riskSuccessRate,actionVarietyScore,bestCount,acceptableCount,badCount,veryBadCount"
        );

        int totalActions = tracker != null ? tracker.totalActions : 0;
        int correctActions = tracker != null ? tracker.correctActions : 0;
        int wrongActions = tracker != null ? tracker.wrongActions : 0;
        float successRate = tracker != null ? tracker.GetSuccessRate() : 0f;
        float averageReactionTime = tracker != null ? tracker.averageReactionTime : 0f;
        float riskPreference = tracker != null ? tracker.GetRiskPreference() : 0f;
        int commitCount = tracker != null ? tracker.commitCount : 0;
        int successfulCommitCount = tracker != null ? tracker.successfulCommitCount : 0;
        float riskSuccessRate = tracker != null ? tracker.GetRiskSuccessRate() : 0f;
        float actionVarietyScore = tracker != null ? tracker.GetActionVarietyScore() : 0f;

        int bestCount = tracker != null ? tracker.bestCount : 0;
        int acceptableCount = tracker != null ? tracker.acceptableCount : 0;
        int badCount = tracker != null ? tracker.badCount : 0;
        int veryBadCount = tracker != null ? tracker.veryBadCount : 0;

        builder.Append(EscapeCsv(sessionId)).Append(',');
        builder.Append(EscapeCsv(participantId)).Append(',');
        builder.Append(EscapeCsv(username)).Append(',');
        builder.Append(EscapeCsv(aiMode)).Append(',');
        builder.Append(conditionGroupIndex).Append(',');
        builder.Append(blockIndex).Append(',');
        builder.Append(EscapeCsv(subsetGroup)).Append(',');
        builder.Append(finalInfluence).Append(',');
        builder.Append(totalActions).Append(',');
        builder.Append(correctActions).Append(',');
        builder.Append(wrongActions).Append(',');
        builder.Append(successRate.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
        builder.Append(averageReactionTime.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
        builder.Append(riskPreference.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
        builder.Append(commitCount).Append(',');
        builder.Append(successfulCommitCount).Append(',');
        builder.Append(riskSuccessRate.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
        builder.Append(actionVarietyScore.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
        builder.Append(bestCount).Append(',');
        builder.Append(acceptableCount).Append(',');
        builder.Append(badCount).Append(',');
        builder.AppendLine(veryBadCount.ToString());

        string csv = builder.ToString();
        File.WriteAllText(path, csv, Encoding.UTF8);
        AndroidPublicStorage.SaveToDownloads(fileName, csv);   // <-- πρόσθεσε αυτό

        Log($"SUMMARY EXPORTED | {path}");
        return path;
    }
    public void ClearLog()
    {
        entries.Clear();
        roundRecords.Clear();

        if (logText != null)
        {
            logText.text = "";
        }
    }

    private string BuildCsv()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("sessionId,participantId,username,roundNumber,scenarioId,aiMode,conditionGroupIndex,blockIndex,subsetGroup,caseType,difficulty,commitTiming,roundPosition,selectedAction,preferredAction,isCorrect,outcomeQuality,nextNpcState,reactionTime,baseInfluenceChange,finalInfluenceChange,adaptiveAdjustment,influenceTotal,matchedCaseId,matchScore,adaptiveReason,behaviourLabel,wasAlreadyResolved,ambiguityGap,secondCaseId,totalActions,successRate,riskPreference,commitCount,successfulCommitCount,riskSuccessRate,actionVarietyScore,averageReactionTime,bestCount,acceptableCount,badCount,veryBadCount,statement,authorityFeedback,aiResponse");

        foreach (SessionRoundRecord record in roundRecords)
        {
            builder.Append(EscapeCsv(record.sessionId)).Append(',');
            builder.Append(EscapeCsv(participantId)).Append(',');
            builder.Append(EscapeCsv(username)).Append(',');
            builder.Append(record.roundNumber).Append(',');
            builder.Append(EscapeCsv(record.scenarioId)).Append(',');
            builder.Append(EscapeCsv(record.aiMode)).Append(',');
            builder.Append(record.conditionGroupIndex).Append(',');
            builder.Append(record.blockIndex).Append(',');
            builder.Append(EscapeCsv(record.subsetGroup)).Append(',');
            builder.Append(EscapeCsv(record.caseType)).Append(',');
            builder.Append(EscapeCsv(record.difficulty)).Append(',');
            builder.Append(EscapeCsv(record.commitTiming)).Append(',');
            builder.Append(EscapeCsv(record.roundPosition)).Append(',');
            builder.Append(EscapeCsv(record.selectedAction)).Append(',');
            builder.Append(EscapeCsv(record.preferredAction)).Append(',');
            builder.Append(record.isCorrect).Append(',');
            builder.Append(EscapeCsv(record.outcomeQuality)).Append(',');
            builder.Append(EscapeCsv(record.nextNpcState)).Append(',');
            builder.Append(record.reactionTime.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(record.baseInfluenceChange).Append(',');
            builder.Append(record.finalInfluenceChange).Append(',');
            builder.Append(record.adaptiveAdjustment).Append(',');
            builder.Append(record.influenceTotal).Append(',');
            builder.Append(EscapeCsv(record.matchedCaseId)).Append(',');
            builder.Append(record.matchScore.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(EscapeCsv(record.adaptiveReason)).Append(',');
            builder.Append(EscapeCsv(record.behaviourLabel)).Append(',');
            builder.Append(record.wasAlreadyResolved).Append(','); 
            builder.Append(record.ambiguityGap.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(EscapeCsv(record.secondCaseId)).Append(',');      
            builder.Append(record.totalActions).Append(',');
            builder.Append(record.successRate.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(record.riskPreference.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(record.commitCount).Append(',');
            builder.Append(record.successfulCommitCount).Append(',');
            builder.Append(record.riskSuccessRate.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(record.actionVarietyScore.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(record.averageReactionTime.ToString("F2", CultureInfo.InvariantCulture)).Append(',');
            builder.Append(record.bestCount).Append(',');
            builder.Append(record.acceptableCount).Append(',');
            builder.Append(record.badCount).Append(',');
            builder.Append(record.veryBadCount).Append(',');
            builder.Append(EscapeCsv(record.statement)).Append(',');
            builder.Append(EscapeCsv(record.authorityFeedback)).Append(',');
            builder.AppendLine(EscapeCsv(record.aiResponse));
        }

        return builder.ToString();
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}

[Serializable]
public class SessionRoundRecord
{
    public string sessionId;
    public string participantId;
    public string username;
    public int roundNumber;
    public string scenarioId;
    public string aiMode;
    public int conditionGroupIndex;
    public int blockIndex;
    public string subsetGroup;
    public string caseType;
    public string difficulty;
    public string commitTiming;
    public string roundPosition;
    public string statement;
    public string selectedAction;
    public string preferredAction;
    public bool isCorrect;
    public string outcomeQuality;
    public string nextNpcState;
    public float reactionTime;
    public int baseInfluenceChange;
    public int finalInfluenceChange;
    public int adaptiveAdjustment;
    public int influenceTotal;
    public string authorityFeedback;
    public string aiResponse;
    public string matchedCaseId;
    public float matchScore;
    public string adaptiveReason;
    public string behaviourLabel;
    public bool wasAlreadyResolved;
    public float ambiguityGap;
    public string secondCaseId;
    public int totalActions;
    public float successRate;
    public float riskPreference;
    public float averageReactionTime;
    public int commitCount;
    public int successfulCommitCount;
    public float riskSuccessRate;
    public float actionVarietyScore;
    public int bestCount;
    public int acceptableCount;
    public int badCount;
    public int veryBadCount;
}