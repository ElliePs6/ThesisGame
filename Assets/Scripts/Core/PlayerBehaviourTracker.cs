using UnityEngine;

public class PlayerBehaviourTracker : MonoBehaviour
{
    [Header("General Metrics")]
    public int totalActions = 0;
    public int correctActions = 0;
    public int wrongActions = 0;

    [Header("Risk Metrics")]
    public int commitCount = 0;
    public int successfulCommitCount = 0;
    public float riskSuccessRate = 0f;

    [Header("Action Variety Metrics")]
    [Range(0f, 1f)]
    public float actionVarietyScore = 0f;

    [Header("Reaction Time")]
    public float totalReactionTime = 0f;
    public float averageReactionTime = 0f;

    [Header("Action Counts")]
    public int probeCount = 0;
    public int challengeCount = 0;
    public int supportCount = 0;
    public int commitActionCount = 0;

    [Header("Outcome Quality Counts")]
    public int bestCount = 0;
    public int acceptableCount = 0;
    public int badCount = 0;
    public int veryBadCount = 0;

    public void RegisterAction(
        PlayerActionType action,
        bool isCorrect,
        float reactionTime,
        ActionOutcomeQuality? quality
    )
    {
        totalActions++;

        if (isCorrect)
            correctActions++;
        else
            wrongActions++;

        totalReactionTime += reactionTime;
        averageReactionTime = totalReactionTime / totalActions;

        switch (action)
        {
            case PlayerActionType.Probe:
                probeCount++;
                break;

            case PlayerActionType.Challenge:
                challengeCount++;
                break;

            case PlayerActionType.Support:
                supportCount++;
                break;

            case PlayerActionType.Commit:
                commitActionCount++;
                commitCount++;

                if (quality.HasValue && IsSuccessfulCommitQuality(quality.Value))
                {
                    successfulCommitCount++;
                }

                break;
        }

        if (quality.HasValue)
        {
            switch (quality.Value)
            {
                case ActionOutcomeQuality.Best:
                    bestCount++;
                    break;

                case ActionOutcomeQuality.Acceptable:
                    acceptableCount++;
                    break;

                case ActionOutcomeQuality.Bad:
                    badCount++;
                    break;

                case ActionOutcomeQuality.VeryBad:
                    veryBadCount++;
                    break;
            }
        }

        riskSuccessRate = GetRiskSuccessRate();
        actionVarietyScore = GetActionVarietyScore();
    }

    private bool IsSuccessfulCommitQuality(ActionOutcomeQuality quality)
    {
        return quality == ActionOutcomeQuality.Best ||
               quality == ActionOutcomeQuality.Acceptable;
    }

    public float GetSuccessRate()
    {
        if (totalActions == 0) return 0f;
        return (float)correctActions / totalActions;
    }

    public float GetRiskPreference()
    {
        if (totalActions == 0) return 0f;
        return (float)commitCount / totalActions;
    }

    public float GetRiskSuccessRate()
    {
        if (commitCount == 0) return 0f;
        return (float)successfulCommitCount / commitCount;
    }

    public float GetActionVarietyScore()
    {
        if (totalActions == 0) return 0f;

        int usedActionTypes = 0;

        if (probeCount > 0) usedActionTypes++;
        if (challengeCount > 0) usedActionTypes++;
        if (supportCount > 0) usedActionTypes++;
        if (commitActionCount > 0) usedActionTypes++;

        return usedActionTypes / 4f;
    }

    public string GetSummary()
    {
        return $"Actions={totalActions}, Correct={correctActions}, Wrong={wrongActions}, AvgReaction={averageReactionTime:F2}s, RiskPreference={GetRiskPreference():F2}, CommitCount={commitCount}, SuccessfulCommit={successfulCommitCount}, RiskSuccessRate={GetRiskSuccessRate():F2}, ActionVariety={GetActionVarietyScore():F2}, Probe={probeCount}, Challenge={challengeCount}, Support={supportCount}, Commit={commitActionCount}, Best={bestCount}, Acceptable={acceptableCount}, Bad={badCount}, VeryBad={veryBadCount}";
    }
}
