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
    public GameObject authorityToastPanel;
    public TMP_Text authorityMessageText;
    public TMP_Text influenceChangeText;
    public TMP_Text feedbackText;
    public TMP_Text roundText;
    public TMP_Text influenceText;
    public TextMeshProUGUI npcSpeechText;
    [Header("NPC Animation")]
    public NpcAnimationController npcAnimationController;
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
    // Πόσο μένει ορατό το feedbackText (π.χ. "Καλή επιλογή...") πριν αλλάξει
    // η ατάκα του NPC. Ζητήθηκε να διαρκεί 6 δευτερόλεπτα ώστε ο παίκτης να
    // προλαβαίνει να το διαβάσει άνετα.
    public float feedbackToNpcSpeechDelay = 6.0f;
    // Πόσο μένει ορατή η ατάκα του NPC (currentNpcMessage) πριν προχωρήσει
    // η ροή στο authority feedback / resolution. Επίσης 6 δευτερόλεπτα.
    public float evaluationPhaseDuration = 6.0f;
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
    private string currentInfluenceChangeMessage;
    private bool isProcessingAction = false;
    private string currentNpcMessage;
    private float roundStartTime;
    private AuthorityToastUI authorityToastUI;

    private void Start()
    {
        if (authorityToastPanel != null)
        {
            authorityToastUI = authorityToastPanel.GetComponent<AuthorityToastUI>();

            if (authorityToastUI == null)
            {
                Debug.LogWarning("GameManager: authorityToastPanel has no AuthorityToastUI component.");
            }
        }

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

        roundPool = DefaultRoundLibrary.GetDefaultRoundPool();
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
        RoundData[] poolForThisSession = roundPool;

        bool isCounterbalanced = GameFlowManager.Instance != null
            && GameFlowManager.Instance.IsCounterbalancedFlow;

        if (isCounterbalanced)
        {
            RoundSubsetGroup targetSubset = GameFlowManager.Instance.CurrentBlockSubsetGroup;

            poolForThisSession = System.Array.FindAll(
                roundPool,
                round => round != null && round.subsetGroup == targetSubset
            );

            sessionRoundCount = poolForThisSession.Length;

            Debug.Log(
                $"Counterbalanced flow: Block {GameFlowManager.Instance.CurrentBlockIndex} | " +
                $"Mode={currentAIMode} | Subset={targetSubset} | {poolForThisSession.Length} rounds available."
            );
        }

        sessionRounds = SessionBuilder.BuildBalancedSession(poolForThisSession, sessionRoundCount);

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

        authorityToastUI?.Hide();
        currentPhase = GamePhase.Presentation;
        currentAuthorityMessage = "";
        currentInfluenceChangeMessage = "";
        currentNpcMessage = $"Round {GetDisplayRoundNumber()}: {sessionRounds[currentRoundIndex].statement}";
        npcAnimationController?.PlayState(
            sessionRounds[currentRoundIndex].npcState
        );
        if (feedbackText != null)
        {
            feedbackText.text = "Επίλεξε την επόμενη κίνησή σου.";
        }

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

        string playerFeedback;
        string aiEvaluationDetails;

        // Το AI evaluation είναι συγχρονισμένο (δεν χρειάζεται δικό του wait) --
        // το τρέχουμε αμέσως ώστε να ξέρουμε το τελικό influence change και
        // να το δείξουμε στο Authority Toast ΠΡΩΤΟ απ' όλα.
        AIEvaluationResult aiResult = aiManager.EvaluateAction(
            currentAIMode,
            GetDisplayRoundNumber(),
            currentRound,
            selectedAction,
            isCorrectAction,
            baseInfluenceChange,
            reactionTime
        );

        // Κρατάμε τα τεχνικά AI details μόνο για logging / dataset.
        aiEvaluationDetails = aiResult.responseText;

        int finalInfluenceChange = aiResult.influenceChange;
        influenceScore = Mathf.Clamp(influenceScore + finalInfluenceChange, 0, 100);

        currentAuthorityMessage = GameTextBuilder.GetAuthorityMessage(
            selectedOutcome,
            isCorrectAction
        );

        currentInfluenceChangeMessage =
            GameTextBuilder.GetInfluenceChangeMessage(finalInfluenceChange);

        ActionOutcomeQuality resolvedQuality = selectedOutcome != null
            ? selectedOutcome.quality
            : (isCorrectAction ? ActionOutcomeQuality.Best : ActionOutcomeQuality.Bad);

        // ΒΗΜΑ 1: ΠΡΩΤΑ εμφανίζεται το Authority Toast Panel (κρίση/βαθμολογία),
        // αμέσως μετά την επιλογή του παίκτη.
        currentPhase = GamePhase.Resolution;
        authorityToastUI?.Show(resolvedQuality, currentAuthorityMessage, finalInfluenceChange);
        UpdateUI();

        gameLogger?.Log(
            $"ROUND {GetDisplayRoundNumber()} | Resolution | Influence Change: {finalInfluenceChange} | Influence Total: {influenceScore}"
        );

        yield return new WaitForSeconds(resolutionPhaseDuration);
        authorityToastUI?.Hide();

        // ΒΗΜΑ 2: ΜΕΤΑ εμφανίζεται το "Ο παίκτης επέλεξε..." κείμενο.
        currentPhase = GamePhase.Action;
        if (feedbackText != null)
        {
            feedbackText.text =
    $"Ο παίκτης επέλεξε: {GameTextBuilder.GetActionDisplayName(selectedAction)}";
        }
        UpdateUI();

        gameLogger?.Log(
            $"ROUND {GetDisplayRoundNumber()} | Action Selected: {selectedAction} | Mode: {currentAIMode}"
        );

        yield return new WaitForSeconds(actionPhaseDuration);

        // ΒΗΜΑ 3: Feedback text -- η ατάκα του NPC ΔΕΝ αλλάζει ακόμα, ώστε
        // ο παίκτης να προλαβαίνει να το διαβάσει.
        currentPhase = GamePhase.Evaluation;

        playerFeedback = GameTextBuilder.BuildPlayerFeedback(
            selectedAction,
            selectedOutcome,
            isCorrectAction
        );

        if (feedbackText != null)
        {
            feedbackText.text = playerFeedback;
        }

        UpdateUI();

        // Delay πριν αλλάξει η ατάκα/κατάσταση του NPC.
        yield return new WaitForSeconds(feedbackToNpcSpeechDelay);

        // ΒΗΜΑ 4: ΤΕΛΟΣ αλλάζει η ατάκα και η κατάσταση/animation του NPC.
        //
        // Σε Adaptive mode, αν το AI έχει βρει ένα αρκετά σίγουρο matched case
        // (βλ. AdaptiveAI.GetAdaptiveNpcLine / dialogueOverrideThreshold), ο
        // παίκτης βλέπει τη δική του, ξεχωριστή ατάκα -- διαφορετική από αυτή
        // που θα έβλεπε στο Static mode για το ίδιο outcome. Αν δεν υπάρχει
        // αρκετά καλό match (ή το mode είναι Static), γίνεται fallback στο
        // κοινό RoundData.outcomes[].npcResponse, όπως πριν.
        currentNpcMessage = !string.IsNullOrEmpty(aiResult.adaptiveNpcLine)
            ? aiResult.adaptiveNpcLine
            : (selectedOutcome != null
                ? selectedOutcome.npcResponse
                : "The NPC reacts to the selected action.");
        if (selectedOutcome != null)
        {
            npcAnimationController?.PlayState(
                selectedOutcome.nextNpcState
            );
        }

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
        // Ενημερώνει τη session memory ΜΟΝΟ στο Adaptive mode, ώστε το επόμενο
        // round να μπορεί να ανιχνεύσει αν τα δικά του contextTags έχουν ήδη
        // "λυθεί" καλά. Μπαίνει εδώ (μετά το EvaluateAction/RegisterAction του
        // ΤΡΕΧΟΝΤΟΣ γύρου, πριν ξεκινήσει ο επόμενος), ώστε να μην επηρεάζεται ο
        // εαυτός του και να επηρεάζεται σωστά μόνο ο επόμενος γύρος.
        aiManager.MarkResolvedIfAdaptive(currentAIMode, currentRound, selectedOutcome?.quality);

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
            aiEvaluationDetails,
            aiResult.matchedCaseId,
            aiResult.matchScore,
            aiResult.adaptiveReason,
            aiResult.behaviourLabel,
            aiResult.wasAlreadyResolved,
            aiResult.ambiguityGap,
            aiResult.secondCaseId,
            behaviourTracker,
            currentRound.caseType.ToString(),
            currentRound.difficulty.ToString(),
            currentRound.commitTiming.ToString(),
            currentRound.roundPosition.ToString(),
            GameFlowManager.Instance != null ? GameFlowManager.Instance.ConditionGroupIndex : -1,
            GameFlowManager.Instance != null && GameFlowManager.Instance.IsCounterbalancedFlow
                ? GameFlowManager.Instance.CurrentBlockIndex
                : -1,
            currentRound.subsetGroup.ToString()
        );

        currentRoundIndex++;
        AdaptiveDifficultyReorderer.ReorderRemaining(
            currentAIMode,
            behaviourTracker,
            sessionRounds,
            currentRoundIndex,
            gameLogger
        );
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


    private void EndPrototype()
    {
        currentPhase = GamePhase.Resolution;

        string finalNpcMessage = "Το πρωτότυπο ολοκληρώθηκε. Όλοι οι γύροι τελείωσαν.";
        if (npcSpeechText != null)
            npcSpeechText.text = finalNpcMessage;

        if (feedbackText != null)
        {
            feedbackText.text = "Φόρτωση αποτελεσμάτων...";
        }

        if (roundText != null)
        {
            roundText.text =
                $"Γύρος: {sessionRounds.Length}/{sessionRounds.Length}";
        }

        if (influenceText != null)
        {
            influenceText.text = $"Επιρροή: {influenceScore}";
        }

        gameLogger?.Log($"END PROTOTYPE | Final Influence: {influenceScore}");

        bool isCounterbalanced = GameFlowManager.Instance != null
            && GameFlowManager.Instance.IsCounterbalancedFlow;

        if (GameFlowManager.Instance != null && behaviourTracker != null)
        {
            if (isCounterbalanced)
            {
                GameFlowManager.Instance.StoreBlockResults(
                    GameFlowManager.Instance.CurrentBlockIndex,
                    currentAIMode,
                    GameFlowManager.Instance.CurrentBlockSubsetGroup,
                    influenceScore,
                    behaviourTracker.correctActions,
                    behaviourTracker.wrongActions,
                    behaviourTracker.GetSuccessRate(),
                    behaviourTracker.averageReactionTime,
                    behaviourTracker.riskSuccessRate,
                    behaviourTracker.actionVarietyScore,
                    behaviourTracker.commitCount
                );
            }
            else
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
            }

            if (gameLogger != null)
            {
                string datasetPath = gameLogger.ExportSessionDataset();
                GameFlowManager.Instance.StoreDatasetPath(datasetPath);

                string summaryPath = isCounterbalanced
                    ? gameLogger.ExportSessionSummary(
                        currentAIMode.ToString(),
                        influenceScore,
                        behaviourTracker,
                        GameFlowManager.Instance.ConditionGroupIndex,
                        GameFlowManager.Instance.CurrentBlockIndex,
                        GameFlowManager.Instance.CurrentBlockSubsetGroup.ToString()
                    )
                    : gameLogger.ExportSessionSummary(
                        currentAIMode.ToString(),
                        influenceScore,
                        behaviourTracker
                    );

                GameFlowManager.Instance.StoreSummaryPath(summaryPath);
            }
        }

        if (isCounterbalanced)
        {
            // Το behaviourTracker ΔΕΝ πρέπει να "κουβαλάει" στατιστικά από
            // το Block 1 μέσα στο Block 2 -- κάθε block μετριέται ανεξάρτητα.
            // (Το behaviourTracker είναι scene-local, οπότε έτσι κι αλλιώς
            // δημιουργείται νέο σε κάθε reload του OfficeSession· αυτό εδώ
            // είναι απλά τεκμηρίωση της πρόθεσης.)
            Invoke(nameof(AdvanceToNextBlockOrResults), 2f);
        }
        else
        {
            Invoke(nameof(GoToResultsScene), 2f);
        }
    }

    private void AdvanceToNextBlockOrResults()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnBlockCompleted();
        }
        else
        {
            Debug.LogWarning("GameFlowManager not found. Cannot advance to next block.");
        }
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
        if (phaseText != null)
        {
            phaseText.text = $"Φάση: {currentPhase} | Λειτουργία: {currentAIMode}";
        }

        if (npcSpeechText != null)
        {
            npcSpeechText.text = currentNpcMessage;
        }

        if (roundText != null)
        {
            roundText.text =
                $"Γύρος: {GetDisplayRoundNumber()}/{sessionRounds.Length}";
        }

        if (influenceText != null)
        {
            influenceText.text = $"Επιρροή: {influenceScore}";
        }
    }
}