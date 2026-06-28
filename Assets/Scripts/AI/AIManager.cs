using UnityEngine;

public class AIManager : MonoBehaviour
{
    [Header("AI References")]
    [SerializeField] private StaticAI staticAI;
    [SerializeField] private AdaptiveAI adaptiveAI;
    [SerializeField] private PlayerBehaviourTracker playerTracker;

    public AIEvaluationResult EvaluateAction(
        AIMode mode,
        int roundNumber,
        RoundData round,
        PlayerActionType selectedAction,
        bool isCorrectAction,
        int baseInfluenceChange,
        float reactionTime
    )
    {
        string responseText;
        int finalInfluenceChange = baseInfluenceChange;
        string matchedCaseId = "";
        float matchScore = 0f;
        string adaptiveReason = "";
        string behaviourLabel = "";      

        if (mode == AIMode.Adaptive && adaptiveAI != null)
        {
            responseText = adaptiveAI.GetAdaptiveResponse(
                roundNumber,
                round,
                selectedAction,
                isCorrectAction,
                playerTracker
            );

            CaseData matchedCase = adaptiveAI.GetLastMatchedCase();
            matchScore = adaptiveAI.GetLastMatchScore();

            finalInfluenceChange = adaptiveAI.GetAdaptiveInfluenceChange(
                selectedAction,
                isCorrectAction,
                playerTracker,
                baseInfluenceChange,
                reactionTime,
                matchedCase,
                matchScore
            );

            matchedCaseId = matchedCase != null
                ? matchedCase.caseId
                : "NO_CASE_MATCHED";

            adaptiveReason = matchedCase != null
                ? matchedCase.rationale
                : "No matched case found.";

            behaviourLabel = adaptiveAI.GetBehaviourLabel(
                playerTracker,
                selectedAction,
                isCorrectAction
            );
        }
        else if (staticAI != null)
        {
            AIEvaluationResult staticResult = staticAI.Evaluate(
                roundNumber,
                selectedAction,
                isCorrectAction,
                baseInfluenceChange
            );
            responseText = staticResult.responseText;
            finalInfluenceChange = staticResult.influenceChange;
            matchedCaseId = staticResult.matchedCaseId;
            matchScore = staticResult.matchScore;
            adaptiveReason = "";
            behaviourLabel = "static_scripted";
        }
        else
        {
            Debug.LogWarning("AIManager: No AI assigned.");

            responseText = "No AI response available.";
            finalInfluenceChange = 0;
            matchedCaseId = "NO_AI_ASSIGNED";
            matchScore = 0f;
            adaptiveReason = "No AI assigned to evaluate action.";
            behaviourLabel = "no_ai";
        }

        return new AIEvaluationResult
        {
            responseText = responseText,
            influenceChange = finalInfluenceChange,
            matchedCaseId = matchedCaseId,
            matchScore = matchScore,
            adaptiveReason = adaptiveReason,
            behaviourLabel = behaviourLabel
        };
    }
}

[System.Serializable]
public class AIEvaluationResult
{
    public string responseText;
    public int influenceChange;
    public string matchedCaseId;
    public float matchScore;

    public string adaptiveReason;
    public string behaviourLabel;
}