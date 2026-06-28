using UnityEngine;

public class AdaptiveAI : MonoBehaviour
{
    [Header("Case Library")]
    public CaseData[] caseLibrary;

    [Header("Matching Weights")]
    [Range(0, 1)]
    public float actionWeight = 0.45f;

    [Range(0, 1)]
    public float npcStateWeight = 0.25f;

    [Range(0, 1)]
    public float contextTagWeight = 0.30f;

    [Header("Response Time Rules")]
    public float fastResponseThreshold = 3.0f;
    public float slowResponseThreshold = 8.0f;

    private CaseData lastMatchedCase;
    private float lastMatchScore;

    private void Awake()
    {
        SetupDefaultCasesIfNeeded();
    }

    private void SetupDefaultCasesIfNeeded()
    {
        if (caseLibrary != null && caseLibrary.Length > 0)
            return;

        caseLibrary = new CaseData[]
        {
            new CaseData
            {
                caseId = "CASE-AVOIDANCE-CLARIFY",
                situation = "NPC avoids clear details while the context contains an information gap.",
                npcState = "avoidance",
                contextTags = new[] { "unclear-details", "low-pressure", "information-gap" },
                recommendedAction = PlayerActionType.Probe,
                rationale = "Clarifying is effective when the NPC is avoidant and the main problem is missing information.",
                alternatives = "Stabilize can keep the interaction calm, but may not resolve the information gap. Confront or Push Forward can increase resistance.",
                expectedOutcome = "The NPC provides a useful detail while remaining somewhat cautious.",
                expertConfidence = 0.85f
            },

            new CaseData
            {
                caseId = "CASE-DEFENSIVE-STABILIZE",
                situation = "NPC becomes defensive and the interaction needs stability before more pressure is applied.",
                npcState = "defensive",
                contextTags = new[] { "tension", "resistance", "stability" },
                recommendedAction = PlayerActionType.Support,
                rationale = "Stabilizing is effective when resistance is high and the interaction needs to remain productive.",
                alternatives = "Clarify may produce limited information. Confront or Push Forward can escalate resistance.",
                expectedOutcome = "The NPC becomes more stable and slightly more cooperative.",
                expertConfidence = 0.82f
            },

            new CaseData
            {
                caseId = "CASE-INCONSISTENT-CONFRONT",
                situation = "NPC gives information that conflicts with an earlier statement.",
                npcState = "inconsistent",
                contextTags = new[] { "contradiction", "evidence", "clarification" },
                recommendedAction = PlayerActionType.Challenge,
                rationale = "Confronting is effective when there is a clear contradiction that needs to be resolved.",
                alternatives = "Clarify can gather more information but may delay resolving the contradiction. Stabilize avoids the key issue. Push Forward is premature.",
                expectedOutcome = "The inconsistency is addressed and the NPC is forced to clarify the conflict.",
                expertConfidence = 0.90f
            },

            new CaseData
            {
                caseId = "CASE-ESCALATED-STABILIZE",
                situation = "NPC is overwhelmed or the interaction becomes unstable.",
                npcState = "escalated",
                contextTags = new[] { "instability", "pressure", "stability" },
                recommendedAction = PlayerActionType.Support,
                rationale = "Stabilizing is effective when the interaction is unstable and more pressure would reduce cooperation.",
                alternatives = "Clarify may increase strain. Confront can escalate the situation further. Push Forward is risky before stability returns.",
                expectedOutcome = "The interaction stabilizes enough to continue.",
                expertConfidence = 0.86f
            },

            new CaseData
            {
                caseId = "CASE-AVOIDANCE-HIDDEN-DETAIL",
                situation = "NPC gives a vague answer that appears to hide an important detail.",
                npcState = "avoidance",
                contextTags = new[] { "unclear-details", "information-gap", "hidden-detail" },
                recommendedAction = PlayerActionType.Probe,
                rationale = "Clarifying is effective when the NPC is vague and a specific missing detail needs to be uncovered.",
                alternatives = "Stabilize can preserve cooperation but may leave the detail hidden. Confront can create resistance too early. Push Forward skips needed information.",
                expectedOutcome = "The NPC reveals a more specific detail and the situation becomes clearer.",
                expertConfidence = 0.84f
            },

            new CaseData
            {
                caseId = "CASE-FIRM-PUSH-FORWARD",
                situation = "NPC reaches a clear decision point and the interaction needs forward movement.",
                npcState = "firm",
                contextTags = new[] { "decision-point", "commitment", "high-clarity" },
                recommendedAction = PlayerActionType.Commit,
                rationale = "Push Forward is effective when the situation is clear enough and delaying would reduce momentum.",
                alternatives = "Clarify may delay unnecessarily. Stabilize may keep things calm but does not resolve the decision point. Confront can create unnecessary friction.",
                expectedOutcome = "The interaction moves toward a clear outcome.",
                expertConfidence = 0.80f
            }
        };
    }

    public int GetAdaptiveInfluenceChange(
        PlayerActionType selectedAction,
        bool isCorrectAction,
        PlayerBehaviourTracker tracker,
        int baseInfluenceChange,
        float reactionTime,
        CaseData matchedCase = null,
        float matchScore = 0f)
    {
        int finalChange = baseInfluenceChange;

        if (tracker == null)
            return ApplyCaseConfidence(finalChange, selectedAction, matchedCase, matchScore);

        float riskPreference = tracker.GetRiskPreference();
        int wrongActions = tracker.wrongActions;

        // Rule 1: If player struggles, reduce negative penalties slightly.
        if (baseInfluenceChange < 0 && wrongActions >= 2)
        {
            finalChange += 1;
        }

        // Rule 2: If player makes a strong choice quickly, reward decisiveness slightly.
        if (isCorrectAction && baseInfluenceChange > 0 && reactionTime <= fastResponseThreshold)
        {
            finalChange += 1;
        }

        // Rule 3: If player makes a bad choice too quickly, punish impulsive mistakes slightly.
        if (!isCorrectAction &&
            baseInfluenceChange < 0 &&
            reactionTime <= fastResponseThreshold &&
            tracker.totalActions > 0)
        {
            finalChange -= 1;
        }

        // Rule 4: If player prefers risky play and fails with Commit, punish more.
        if (!isCorrectAction && selectedAction == PlayerActionType.Commit && riskPreference >= 0.40f)
        {
            finalChange -= 2;
        }

        return ApplyCaseConfidence(finalChange, selectedAction, matchedCase, matchScore);
    }
    public string GetAdaptiveResponse(
        int roundNumber,
        RoundData round,
        PlayerActionType action,
        bool isCorrectAction,
        PlayerBehaviourTracker tracker)
    {
        lastMatchedCase = FindBestCase(round, action, out lastMatchScore);

        if (tracker == null)
        {
            return BuildCaseResponse(roundNumber, action, isCorrectAction, lastMatchedCase, lastMatchScore);
        }

        float successRate = tracker.GetSuccessRate();
        float riskPreference = tracker.GetRiskPreference();
        string caseResponse = BuildCaseResponse(roundNumber, action, isCorrectAction, lastMatchedCase, lastMatchScore);

        if (isCorrectAction && successRate >= 0.70f)
        {
            return $"{caseResponse} Player pattern: consistent high performance detected.";
        }

        if (!isCorrectAction && tracker.wrongActions >= 2)
        {
            return $"{caseResponse} Player pattern: repeated difficulty detected, so the response is softened.";
        }

        if (!isCorrectAction && action == PlayerActionType.Commit && riskPreference >= 0.40f)
        {
            return $"{caseResponse} Player pattern: repeated high-risk choices detected, so the system pushes back harder.";
        }

        return caseResponse;
    }

    public CaseData GetLastMatchedCase()
    {
        return lastMatchedCase;
    }

    public float GetLastMatchScore()
    {
        return lastMatchScore;
    }

    public string GetBehaviourLabel(
        PlayerBehaviourTracker tracker,
        PlayerActionType action,
        bool isCorrectAction
    )
    {
        if (tracker == null)
            return "no_tracker";

        if (!isCorrectAction &&
            action == PlayerActionType.Commit &&
            tracker.GetRiskPreference() >= 0.40f)
        {
            return "high_risk_failure";
        }

        if (isCorrectAction && tracker.GetSuccessRate() >= 0.70f)
        {
            return "consistent_high_performance";
        }

        if (!isCorrectAction && tracker.wrongActions >= 2)
        {
            return "repeated_difficulty";
        }

        if (action == PlayerActionType.Commit && tracker.GetRiskPreference() >= 0.40f)
        {
            return "risk_seeking";
        }

        return "neutral";
    }

    private CaseData FindBestCase(RoundData round, PlayerActionType action, out float bestScore)
    {
        bestScore = 0f;
        CaseData bestCase = null;

        if (round == null || caseLibrary == null)
            return null;

        foreach (CaseData candidate in caseLibrary)
        {
            if (candidate == null)
                continue;

            float score = ScoreCase(candidate, round, action);
            if (score > bestScore)
            {
                bestScore = score;
                bestCase = candidate;
            }
        }

        return bestCase;
    }

    private float ScoreCase(CaseData candidate, RoundData round, PlayerActionType action)
    {
        float score = 0f;

        if (candidate.recommendedAction == action)
            score += actionWeight;

        if (!string.IsNullOrWhiteSpace(candidate.npcState) &&
            !string.IsNullOrWhiteSpace(round.npcState) &&
            string.Equals(candidate.npcState.Trim(), round.npcState.Trim(), System.StringComparison.OrdinalIgnoreCase))
        {
            score += npcStateWeight;
        }

        score += contextTagWeight * GetTagSimilarity(candidate.contextTags, round.contextTags);
        return Mathf.Clamp01(score);
    }

    private float GetTagSimilarity(string[] caseTags, string[] roundTags)
    {
        if (caseTags == null || roundTags == null || caseTags.Length == 0 || roundTags.Length == 0)
            return 0f;

        int matches = 0;

        foreach (string caseTag in caseTags)
        {
            if (string.IsNullOrWhiteSpace(caseTag))
                continue;

            foreach (string roundTag in roundTags)
            {
                if (string.IsNullOrWhiteSpace(roundTag))
                    continue;

                if (string.Equals(caseTag.Trim(), roundTag.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    matches++;
                    break;
                }
            }
        }

        return Mathf.Clamp01((float)matches / Mathf.Max(caseTags.Length, roundTags.Length));
    }

    private string BuildCaseResponse(int roundNumber, PlayerActionType action, bool isCorrectAction, CaseData matchedCase, float matchScore)
    {
        if (matchedCase == null)
        {
            return $"Adaptive AI: Round {roundNumber} used behaviour metrics, but no similar case was found.";
        }

        string fit = isCorrectAction ? "fits" : "does not fit";
        return $"Adaptive AI: Matched {matchedCase.caseId} ({matchScore:P0}). Action {action} {fit} the case rationale: {matchedCase.rationale} Expected outcome: {matchedCase.expectedOutcome}";
    }

    private int ApplyCaseConfidence(int influenceChange, PlayerActionType selectedAction, CaseData matchedCase, float matchScore)
    {
        if (matchedCase == null || matchScore < 0.60f)
            return influenceChange;

        if (selectedAction == matchedCase.recommendedAction && influenceChange > 0)
            return influenceChange + Mathf.RoundToInt(matchedCase.expertConfidence * 2f);

        if (selectedAction != matchedCase.recommendedAction && influenceChange < 0)
            return influenceChange - 1;

        return influenceChange;
    }
}
