using UnityEngine;

public class AIManager : MonoBehaviour
{
    [Header("AI References")]
    [SerializeField] private StaticAI staticAI;
    [SerializeField] private AdaptiveAI adaptiveAI;
    [SerializeField] private PlayerBehaviourTracker playerTracker;

    public AIEvaluationResult EvaluateAction(AIMode mode, int roundNumber,  RoundData round, PlayerActionType selectedAction,
        bool isCorrectAction, int baseInfluenceChange,float reactionTime )
    {
        string responseText;
        int finalInfluenceChange = baseInfluenceChange;
        string matchedCaseId = "";
        float matchScore = 0f;
        string adaptiveReason = "";
        string behaviourLabel = "";
        string adaptiveNpcLine = null;
        bool wasAlreadyResolved = false;
        float ambiguityGap = -1f; // -1 = δεν υπολογίστηκε
        string secondCaseId = "";

        if (mode == AIMode.Adaptive && adaptiveAI != null)
        {
            responseText = adaptiveAI.GetAdaptiveResponse(roundNumber,round,selectedAction,isCorrectAction,playerTracker);

            CaseData matchedCase = adaptiveAI.GetLastMatchedCase();
            matchScore = adaptiveAI.GetLastMatchScore();

            CaseData secondCase = adaptiveAI.GetLastSecondCase();
                if (secondCase != null)
                {
                    ambiguityGap = matchScore - adaptiveAI.GetLastSecondScore();
                    secondCaseId = secondCase.caseId;
                }
            finalInfluenceChange = adaptiveAI.GetAdaptiveInfluenceChange(selectedAction,isCorrectAction,playerTracker,baseInfluenceChange,reactionTime,matchedCase,matchScore);

            matchedCaseId = matchedCase != null
                ? matchedCase.caseId
                : "NO_CASE_MATCHED";

            adaptiveReason = matchedCase != null
                ? matchedCase.rationale
                : "No matched case found.";

            behaviourLabel = adaptiveAI.GetBehaviourLabel(playerTracker,selectedAction,isCorrectAction,reactionTime);
            adaptiveNpcLine = adaptiveAI.GetAdaptiveNpcLine(round);
            wasAlreadyResolved = adaptiveAI.GetLastWasAlreadyResolved(round);
        }
        else if (staticAI != null)
        {
            AIEvaluationResult staticResult = staticAI.Evaluate(roundNumber,selectedAction,isCorrectAction,baseInfluenceChange);
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
            behaviourLabel = behaviourLabel,
            adaptiveNpcLine = adaptiveNpcLine,
            wasAlreadyResolved = wasAlreadyResolved,
            ambiguityGap = ambiguityGap,
            secondCaseId = secondCaseId
        };
    }
    /// <summary>
    /// Ενημερώνει τη session memory του Adaptive AI (βλ. AdaptiveAI.MarkResolved)
    /// αμέσως μετά την αξιολόγηση ενός γύρου, ώστε το ΕΠΟΜΕΝΟ round να μπορεί
    /// να ανιχνεύσει αν κάποιο από τα δικά του contextTags έχει ήδη "λυθεί"
    /// καλά νωρίτερα στο ίδιο block. Στο Static mode δεν κάνει τίποτα -- το
    /// Static mode δεν έχει καθόλου πρόσβαση σε αυτή τη μνήμη.
    /// </summary>
    public void MarkResolvedIfAdaptive(AIMode mode, RoundData round, ActionOutcomeQuality? quality)
    {
        if (mode == AIMode.Adaptive && adaptiveAI != null && quality.HasValue && round != null)
        {
            adaptiveAI.MarkResolved(round.contextTags, quality.Value, round.difficulty);
        }
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

    /// <summary>
    /// Player-facing NPC ατάκα, ορισμένη ΜΟΝΟ όταν το Adaptive AI έχει
    /// αρκετά σίγουρο matched case. Null σε Static mode ή όταν δεν υπάρχει
    /// αρκετά καλό match -- ο GameManager κάνει fallback στο στατικό κείμενο.
    /// </summary>
    public string adaptiveNpcLine;
    /// <summary>
    /// True όταν το Adaptive AI αναγνώρισε ότι τα contextTags του τρέχοντος
    /// round έχουν ήδη "λυθεί" καλά νωρίτερα στο ίδιο block (βλ.
    /// AdaptiveAI.MarkResolved/WasAlreadyResolved). Πάντα false στο Static
    /// mode, αφού δεν έχει καθόλου πρόσβαση σε αυτή τη μνήμη. Χρησιμοποιείται
    /// για logging/dataset, ώστε να αναλυθεί μετά πόσο συχνά συνέβη.
    /// </summary>
    public bool wasAlreadyResolved;
    public float ambiguityGap;
    public string secondCaseId;
}