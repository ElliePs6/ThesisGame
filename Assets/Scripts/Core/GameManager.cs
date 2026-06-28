using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public enum GamePhase
{
    Presentation,
    Action,
    Evaluation,
    Resolution
}

public enum PlayerActionType
{
    Probe,
    Challenge,
    Support,
    Commit
}

public enum AIMode
{
    Static,
    Adaptive
}

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text phaseText;
    public TMP_Text primaryNpcText;
    public TMP_Text authorityText;
    public TMP_Text feedbackText;
    public TMP_Text roundText;
    public TMP_Text influenceText;

    [Header("AI Manager")]
    public AIManager aiManager;
    public AIMode currentAIMode = AIMode.Static;

    [Header("Logger")]
    public GameLogger gameLogger;

    [Header("Progress")]
    [Range(0, 100)]
    public int influenceScore = 50;

    [Header("Timing")]
    public float actionPhaseDuration = 1.0f;
    public float evaluationPhaseDuration = 1.2f;
    public float resolutionPhaseDuration = 1.5f;

    [Header("Tracking")]
    public PlayerBehaviourTracker behaviourTracker;

    [Header("Round Pool")]
    [Tooltip("All available rounds/cases. This is the full pool, not necessarily what the player will play in one session.")]
    [FormerlySerializedAs("rounds")]
    public RoundData[] roundPool;

    [Header("Session Rounds")]
    [Tooltip("How many rounds should be selected for one session. For this prototype, keep this at 10.")]
    [Range(1, 30)]
    public int sessionRoundCount = 10;

    [Tooltip("Rounds selected for the current session. For now this takes up to 10 rounds from the round pool.")]
    public RoundData[] sessionRounds;

    private int currentRoundIndex = 0;
    private GamePhase currentPhase = GamePhase.Presentation;
    private string currentAuthorityMessage;
    private bool isProcessingAction = false;
    private string currentNpcMessage;
    private float roundStartTime;

    private void Start()
    {
        if (GameFlowManager.Instance != null)
        {
            currentAIMode = GameFlowManager.Instance.SelectedMode;
            Debug.Log("OfficeSession started with AI mode: " + currentAIMode);
        }
        else
        {
            currentAIMode = AIMode.Static;
            Debug.LogWarning("GameFlowManager not found. Defaulting to Static mode.");
        }

        SetupDefaultRoundPoolIfNeeded();
        BuildSessionFromRoundPool();
        StartRound();
    }

    private void SetupDefaultRoundPoolIfNeeded()
    {
        if (HasValidRounds(roundPool))
            return;

        roundPool = new RoundData[10];

        roundPool[0] = new RoundData
        {
            scenarioId = "ROUND-001",
            statement = "The NPC avoids giving clear details and keeps the response short.",
            caseType = CaseType.InformationGap,
            difficulty = DifficultyLevel.Easy,
            commitTiming = CommitTiming.Early,
            roundPosition = RoundPosition.Opening,
            npcState = "avoidance",
            contextTags = new[] { "unclear-details", "low-pressure", "information-gap" },
            preferredAction = PlayerActionType.Probe,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "The NPC provides a useful detail, but still remains cautious.",
                    nextNpcState = "defensive",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "The NPC becomes slightly more cooperative, but the main information gap remains.",
                    nextNpcState = "avoidance",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The NPC resists the pressure and becomes less cooperative.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "The player pushes forward too early, causing the interaction to lose flexibility.",
                    nextNpcState = "escalated",
                    influenceChange = -3
                }
            }
        };

        roundPool[1] = new RoundData
        {
            scenarioId = "ROUND-002",
            statement = "The NPC becomes tense and defensive during the interaction.",
            caseType = CaseType.Defensive,
            difficulty = DifficultyLevel.Medium,
            commitTiming = CommitTiming.Early,
            roundPosition = RoundPosition.Development,
            npcState = "defensive",
            contextTags = new[] { "tension", "resistance", "stability" },
            preferredAction = PlayerActionType.Support,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "The NPC becomes more stable and shows a small increase in cooperation.",
                    nextNpcState = "avoidance",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "The NPC gives a limited answer, but the defensive tone remains.",
                    nextNpcState = "defensive",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The NPC interprets the pressure negatively and becomes more resistant.",
                    nextNpcState = "escalated",
                    influenceChange = -2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "The player pushes forward before stability is restored, increasing resistance.",
                    nextNpcState = "escalated",
                    influenceChange = -3
                }
            }
        };

        roundPool[2] = new RoundData
        {
            scenarioId = "ROUND-003",
            statement = "The NPC gives information that conflicts with an earlier statement.",
            caseType = CaseType.Contradiction,
            difficulty = DifficultyLevel.Medium,
            commitTiming = CommitTiming.Middle,
            roundPosition = RoundPosition.Development,
            npcState = "inconsistent",
            contextTags = new[] { "contradiction", "evidence", "clarification" },
            preferredAction = PlayerActionType.Challenge,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "The inconsistency is addressed directly and the NPC is forced to clarify the conflict.",
                    nextNpcState = "defensive",
                    influenceChange = 5
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "The NPC gives more information, but the contradiction is not fully resolved.",
                    nextNpcState = "inconsistent",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The NPC remains cooperative, but the key inconsistency is left unchallenged.",
                    nextNpcState = "inconsistent",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "The player pushes forward before resolving the contradiction, weakening the decision path.",
                    nextNpcState = "defensive",
                    influenceChange = -3
                }
            }
        };

        roundPool[3] = new RoundData
        {
            scenarioId = "ROUND-004",
            statement = "The NPC appears overwhelmed and the interaction becomes unstable.",
            caseType = CaseType.Escalated,
            difficulty = DifficultyLevel.Hard,
            commitTiming = CommitTiming.Middle,
            roundPosition = RoundPosition.Development,
            npcState = "escalated",
            contextTags = new[] { "instability", "pressure", "stability" },
            preferredAction = PlayerActionType.Support,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "The interaction stabilizes and the NPC becomes able to continue.",
                    nextNpcState = "defensive",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The additional questioning increases the strain and produces little useful information.",
                    nextNpcState = "escalated",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "The pressure escalates the situation further and cooperation drops sharply.",
                    nextNpcState = "escalated",
                    influenceChange = -3
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The player pushes forward before the situation is stable.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                }
            }
        };

        roundPool[4] = new RoundData
        {
            scenarioId = "ROUND-005",
            statement = "The NPC gives a vague answer that seems to hide an important detail.",
            caseType = CaseType.HiddenDetail,
            difficulty = DifficultyLevel.Medium,
            commitTiming = CommitTiming.Early,
            roundPosition = RoundPosition.Development,
            npcState = "avoidance",
            contextTags = new[] { "unclear-details", "information-gap", "hidden-detail" },
            preferredAction = PlayerActionType.Probe,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "The NPC reveals a more specific detail and the situation becomes clearer.",
                    nextNpcState = "defensive",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "The NPC stays cooperative, but the hidden detail remains unclear.",
                    nextNpcState = "avoidance",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The NPC becomes guarded before the missing detail is properly explored.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "The player pushes forward while a key detail is still missing.",
                    nextNpcState = "escalated",
                    influenceChange = -3
                }
            }
        };

        roundPool[5] = new RoundData
        {
            scenarioId = "ROUND-006",
            statement = "The NPC reaches a clear decision point and the interaction needs forward movement.",
            caseType = CaseType.DecisionPoint,
            difficulty = DifficultyLevel.Medium,
            commitTiming = CommitTiming.Late,
            roundPosition = RoundPosition.Closing,
            npcState = "firm",
            contextTags = new[] { "decision-point", "commitment", "high-clarity" },
            preferredAction = PlayerActionType.Commit,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "The player pushes forward at the right moment and the interaction moves toward a clear outcome.",
                    nextNpcState = "resolved",
                    influenceChange = 6
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The player asks for more information even though the situation is already clear.",
                    nextNpcState = "firm",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "The NPC remains stable, but the interaction still needs forward movement.",
                    nextNpcState = "firm",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The player creates unnecessary friction at the decision point.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                }
            }
        };

        roundPool[6] = new RoundData
        {
            scenarioId = "ROUND-007",
            statement = "The NPC gives a cooperative answer, but one important detail still seems missing.",
            caseType = CaseType.HiddenDetail,
            difficulty = DifficultyLevel.Medium,
            commitTiming = CommitTiming.Middle,
            roundPosition = RoundPosition.Development,
            npcState = "cooperative-but-incomplete",
            contextTags = new[] { "partial-cooperation", "hidden-detail", "clarification" },
            preferredAction = PlayerActionType.Probe,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "The NPC adds the missing detail and the situation becomes much clearer.",
                    nextNpcState = "cooperative",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "The NPC feels understood, but the missing detail is still not fully explored.",
                    nextNpcState = "cooperative-but-incomplete",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The NPC becomes confused by the sudden pressure and gives a less useful answer.",
                    nextNpcState = "defensive",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "The player moves forward with some risk. The interaction continues, but the missing detail may still matter later.",
                    nextNpcState = "uncertain",
                    influenceChange = 1
                }
            }
        };

        roundPool[7] = new RoundData
        {
            scenarioId = "ROUND-008",
            statement = "The NPC repeats a claim that sounds confident, but the available information is still uncertain.",
            caseType = CaseType.DecisionPoint,
            difficulty = DifficultyLevel.Hard,
            commitTiming = CommitTiming.Early,
            roundPosition = RoundPosition.Development,
            npcState = "confident-but-uncertain",
            contextTags = new[] { "premature-decision", "risk", "uncertainty" },
            preferredAction = PlayerActionType.Probe,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "The NPC slows down and provides additional context before any decision is made.",
                    nextNpcState = "reflective",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "The NPC remains calm, but the uncertainty behind the claim is not fully examined.",
                    nextNpcState = "confident-but-uncertain",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "The NPC is pushed to defend the claim, which reveals some useful information but adds tension.",
                    nextNpcState = "defensive",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "The player commits too early based on uncertain information, creating a weak decision.",
                    nextNpcState = "escalated",
                    influenceChange = -4
                }
            }
        };

        roundPool[8] = new RoundData
        {
            scenarioId = "ROUND-009",
            statement = "The NPC reacts strongly after feeling misunderstood and the interaction begins to escalate.",
            caseType = CaseType.Escalated,
            difficulty = DifficultyLevel.Hard,
            commitTiming = CommitTiming.Middle,
            roundPosition = RoundPosition.Development,
            npcState = "angry",
            contextTags = new[] { "escalation", "misunderstanding", "emotional-intensity" },
            preferredAction = PlayerActionType.Support,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "The NPC feels acknowledged and the emotional intensity begins to drop.",
                    nextNpcState = "defensive",
                    influenceChange = 5
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The NPC experiences the question as pressure and becomes less willing to continue.",
                    nextNpcState = "escalated",
                    influenceChange = -2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "The NPC becomes more upset and the interaction loses cooperation.",
                    nextNpcState = "escalated",
                    influenceChange = -4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The player tries to force progress while the NPC is still emotionally escalated.",
                    nextNpcState = "resistant",
                    influenceChange = -3
                }
            }
        };

        roundPool[9] = new RoundData
        {
            scenarioId = "ROUND-010",
            statement = "The NPC has clarified the main issue and is ready for a concrete next step.",
            caseType = CaseType.DecisionPoint,
            difficulty = DifficultyLevel.Medium,
            commitTiming = CommitTiming.Late,
            roundPosition = RoundPosition.Closing,
            npcState = "ready",
            contextTags = new[] { "decision-point", "resolution", "high-clarity" },
            preferredAction = PlayerActionType.Commit,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "The player commits at the right moment and the interaction reaches a clear resolution.",
                    nextNpcState = "resolved",
                    influenceChange = 6
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "The NPC remains stable, but the interaction still needs a concrete next step.",
                    nextNpcState = "ready",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The player keeps asking questions even though the issue is already clear.",
                    nextNpcState = "impatient",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "The player creates unnecessary friction at a point where resolution was possible.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                }
            }
        };
    }

    private bool HasValidRounds(RoundData[] candidateRounds)
    {
        if (candidateRounds == null || candidateRounds.Length == 0)
            return false;

        foreach (RoundData round in candidateRounds)
        {
            if (round == null)
                return false;

            if (string.IsNullOrWhiteSpace(round.statement))
                return false;

            if (round.outcomes == null || round.outcomes.Length == 0)
                return false;
        }

        return true;
    }

    private void BuildSessionFromRoundPool()
    {
        sessionRounds = SessionBuilder.BuildBalancedSession(roundPool, sessionRoundCount);

        if (sessionRounds == null || sessionRounds.Length == 0)
        {
            Debug.LogWarning("No session rounds were created. The prototype will end immediately.");
            sessionRounds = new RoundData[0];
            return;
        }

        Debug.Log($"Session created with {sessionRounds.Length} balanced rounds from round pool.");
    }

    public void StartRound()
    {
        if (currentRoundIndex >= sessionRounds.Length)
        {
            EndPrototype();
            return;
        }

        currentPhase = GamePhase.Presentation;
        currentAuthorityMessage = "Authority is waiting to evaluate.";
        currentNpcMessage = $"Round {GetDisplayRoundNumber()}: {sessionRounds[currentRoundIndex].statement}";
        feedbackText.text = "Choose an action.";
        isProcessingAction = false;
        roundStartTime = Time.time;

        UpdateUI();

        gameLogger?.Log(
            $"START ROUND {GetDisplayRoundNumber()} | Statement: \"{sessionRounds[currentRoundIndex].statement}\" | Preferred: {sessionRounds[currentRoundIndex].preferredAction}"
        );
    }

    public void OnProbeSelected()
    {
        TryHandlePlayerAction(PlayerActionType.Probe);
    }

    public void OnChallengeSelected()
    {
        TryHandlePlayerAction(PlayerActionType.Challenge);
    }

    public void OnSupportSelected()
    {
        TryHandlePlayerAction(PlayerActionType.Support);
    }

    public void OnCommitSelected()
    {
        TryHandlePlayerAction(PlayerActionType.Commit);
    }

    public void OnExportDatasetSelected()
    {
        if (gameLogger == null)
            return;

        string exportPath = gameLogger.ExportSessionDataset();

        if (feedbackText != null)
        {
            feedbackText.text = $"Dataset exported: {exportPath}";
        }
    }

    private void TryHandlePlayerAction(PlayerActionType action)
    {
        if (currentPhase != GamePhase.Presentation || isProcessingAction)
            return;

        StartCoroutine(ProcessPlayerAction(action));
    }

    private IEnumerator ProcessPlayerAction(PlayerActionType selectedAction)
    {
        isProcessingAction = true;

        RoundData currentRound = sessionRounds[currentRoundIndex];
        ActionOutcome selectedOutcome = currentRound.GetOutcome(selectedAction);

        if (selectedOutcome != null)
        {
            gameLogger?.Log(
                $"OUTCOME | Action={selectedOutcome.action} | Quality={selectedOutcome.quality} | NPC='{selectedOutcome.npcResponse}' | NextState={selectedOutcome.nextNpcState} | Influence={selectedOutcome.influenceChange}"
            );
        }
        else
        {
            gameLogger?.Log($"OUTCOME | No outcome found for action {selectedAction}");
        }

        bool isCorrectAction = selectedOutcome != null
            ? selectedOutcome.quality == ActionOutcomeQuality.Best
            : selectedAction == currentRound.preferredAction;

        float reactionTime = Time.time - roundStartTime;

        int baseInfluenceChange = selectedOutcome != null
            ? selectedOutcome.influenceChange
            : GetInfluenceChange(selectedAction, isCorrectAction);

        int finalInfluenceChange = baseInfluenceChange;
        string evaluationResponse;

        currentPhase = GamePhase.Action;
        feedbackText.text = $"Player selected: {selectedAction}";
        UpdateUI();

        gameLogger?.Log(
            $"ROUND {GetDisplayRoundNumber()} | Action Selected: {selectedAction} | Mode: {currentAIMode}"
        );

        yield return new WaitForSeconds(actionPhaseDuration);

        currentPhase = GamePhase.Evaluation;

        AIEvaluationResult aiResult = aiManager.EvaluateAction(
            currentAIMode,
            GetDisplayRoundNumber(),
            currentRound,
            selectedAction,
            isCorrectAction,
            baseInfluenceChange,
            reactionTime
        );

        currentNpcMessage = selectedOutcome != null
            ? selectedOutcome.npcResponse
            : aiResult.responseText;

        evaluationResponse = selectedOutcome != null
            ? $"{selectedOutcome.quality}: {selectedOutcome.npcResponse}\n\n{aiResult.responseText}"
            : aiResult.responseText;

        finalInfluenceChange = aiResult.influenceChange;

        feedbackText.text = evaluationResponse;
        UpdateUI();

        gameLogger?.Log(
            $"ROUND {GetDisplayRoundNumber()} | Evaluation | Correct Action: {isCorrectAction} | BaseChange: {baseInfluenceChange} | FinalChange: {finalInfluenceChange}"
        );

        yield return new WaitForSeconds(evaluationPhaseDuration);

        behaviourTracker?.RegisterAction(
            selectedAction,
            isCorrectAction,
            reactionTime,
            selectedOutcome != null ? selectedOutcome.quality : null
        );

        gameLogger?.Log(
            $"TRACKER | ReactionTime={reactionTime:F2}s | {behaviourTracker?.GetSummary()}"
        );

        influenceScore = Mathf.Clamp(influenceScore + finalInfluenceChange, 0, 100);

        currentPhase = GamePhase.Resolution;
        currentAuthorityMessage = GetAuthorityFeedback(selectedAction, isCorrectAction, finalInfluenceChange);
        UpdateUI();

        gameLogger?.Log(
            $"ROUND {GetDisplayRoundNumber()} | Resolution | Influence Change: {finalInfluenceChange} | Influence Total: {influenceScore}"
        );

        gameLogger?.LogRoundResult(
            GetDisplayRoundNumber(),
            currentRound.scenarioId,
            currentAIMode.ToString(),
            currentRound.statement,
            selectedAction.ToString(),
            currentRound.preferredAction.ToString(),
            isCorrectAction,
            selectedOutcome != null ? selectedOutcome.quality.ToString() : "",
            selectedOutcome != null ? selectedOutcome.nextNpcState : "",
            reactionTime,
            baseInfluenceChange,
            finalInfluenceChange,
            finalInfluenceChange - baseInfluenceChange,
            influenceScore,
            currentAuthorityMessage,
            evaluationResponse,
            aiResult.matchedCaseId,
            aiResult.matchScore,
            aiResult.adaptiveReason,
            aiResult.behaviourLabel,
            behaviourTracker,
            currentRound.caseType.ToString(),
            currentRound.difficulty.ToString(),
            currentRound.commitTiming.ToString(),
            currentRound.roundPosition.ToString()
        );

        yield return new WaitForSeconds(resolutionPhaseDuration);

        currentRoundIndex++;
        StartRound();
    }

    private int GetInfluenceChange(PlayerActionType selectedAction, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            switch (selectedAction)
            {
                case PlayerActionType.Probe:
                    return 4;
                case PlayerActionType.Challenge:
                    return 5;
                case PlayerActionType.Support:
                    return 4;
                case PlayerActionType.Commit:
                    return 6;
                default:
                    return 2;
            }
        }

        switch (selectedAction)
        {
            case PlayerActionType.Probe:
                return -1;
            case PlayerActionType.Challenge:
                return -2;
            case PlayerActionType.Support:
                return -1;
            case PlayerActionType.Commit:
                return -3;
            default:
                return -1;
        }
    }

    private string GetAuthorityFeedback(PlayerActionType selectedAction, bool isCorrectAction, int influenceChange)
    {
        if (isCorrectAction)
        {
            return $"Authority: {selectedAction} matched the needs of the round. Influence increased by {influenceChange}.";
        }

        return $"Authority: {selectedAction} did not fit the needs of the round. Influence changed by {influenceChange}.";
    }

    private void EndPrototype()
    {
        currentPhase = GamePhase.Resolution;

        primaryNpcText.text = "Prototype complete. All rounds finished.";
        authorityText.text = $"Authority final evaluation: Influence ended at {influenceScore}.";
        feedbackText.text = "Loading results...";
        roundText.text = $"Round: {sessionRounds.Length}/{sessionRounds.Length}";
        influenceText.text = $"Influence: {influenceScore}";

        gameLogger?.Log($"END PROTOTYPE | Final Influence: {influenceScore}");

        if (GameFlowManager.Instance != null && behaviourTracker != null)
        {
            GameFlowManager.Instance.StoreSessionResults(
                influenceScore,
                behaviourTracker.correctActions,
                behaviourTracker.wrongActions,
                behaviourTracker.GetSuccessRate(),
                behaviourTracker.averageReactionTime,
                behaviourTracker.riskSuccessRate,
                behaviourTracker.actionVarietyScore,
                behaviourTracker.commitCount
            );

            if (gameLogger != null)
            {
                string datasetPath = gameLogger.ExportSessionDataset();
                GameFlowManager.Instance.StoreDatasetPath(datasetPath);

                string summaryPath = gameLogger.ExportSessionSummary(
                    currentAIMode.ToString(),
                    influenceScore,
                    behaviourTracker
                );
                GameFlowManager.Instance.StoreSummaryPath(summaryPath);
            }
        }

        Invoke(nameof(GoToResultsScene), 2f);
    }

    private void GoToResultsScene()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToResults();
        }
        else
        {
            Debug.LogWarning("GameFlowManager not found. Cannot go to Results scene.");
        }
    }

    private int GetDisplayRoundNumber()
    {
        return currentRoundIndex + 1;
    }

    private void UpdateUI()
    {
        phaseText.text = $"Phase: {currentPhase} | Mode: {currentAIMode}";

        if (primaryNpcText != null)
        {
            primaryNpcText.text = currentNpcMessage;
        }

        if (authorityText != null)
        {
            authorityText.text = currentAuthorityMessage;
        }

        if (roundText != null)
        {
            roundText.text = $"Round: {GetDisplayRoundNumber()}";
        }

        if (influenceText != null)
        {
            influenceText.text = $"Influence: {influenceScore}";
        }
    }
}
