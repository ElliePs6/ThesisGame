using UnityEngine;
using System.Collections.Generic;

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

    [Header("Player-Facing Dialogue")]
    [Range(0, 1)]
    [Tooltip("Ελάχιστο matchScore για να εμφανιστεί το adaptiveNpcLine ενός case στον παίκτη αντί για το στατικό RoundData.npcResponse. Ίδιο threshold με το ApplyCaseConfidence, ώστε η ορατή αλλαγή διαλόγου να συμπίπτει με το πότε το AI 'είναι σίγουρο' για το match.")]
    public float dialogueOverrideThreshold = 0.60f;

    private CaseData lastMatchedCase;
    private float lastMatchScore;

    // --- Προστέθηκαν για το Revise: κρατάμε και το 2ο καλύτερο match ---
    private CaseData lastSecondCase;
    private float lastSecondScore;

    /// <summary>
    /// Μια "μνήμη" ενός round που λύθηκε καλά νωρίτερα στο block: κρατάει
    /// ΟΛΟ το tag-set του (όχι μεμονωμένα tags), ώστε το WasAlreadyResolved
    /// να απαιτεί πραγματική ομοιότητα με ΕΝΑ συγκεκριμένο προηγούμενο
    /// round -- όχι απλά ένα τυχαίο κοινό tag.
    /// </summary>
    private class ResolvedRoundMemory
    {
        public string[] Tags;
        public DifficultyLevel Difficulty;
    }

    private readonly List<ResolvedRoundMemory> resolvedContextTags = new List<ResolvedRoundMemory>();

    /// <summary>
    /// Ελάχιστος αριθμός κοινών tags ανάμεσα στο τρέχον round και ένα
    /// προηγούμενο "λυμένο" round, ώστε να θεωρηθεί πραγματικά όμοιο.
    /// </summary>
    private const int MinSharedTagsForMemory = 2;

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
                expertConfidence = 0.85f,
                adaptiveNpcLine = "Χμ... εντάξει. Ίσως υπάρχει κάτι που δεν ανέφερα πριν. Ας το ξαναδούμε."
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
                expertConfidence = 0.82f,
                adaptiveNpcLine = "Δεν ήθελα να ακουστώ έτσι. Απλώς νιώθω λίγο πιεσμένος αυτή τη στιγμή."
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
                expertConfidence = 0.90f,
                adaptiveNpcLine = "Περίμενε... έχεις δίκιο, αυτό δεν ταιριάζει με ό,τι είπα πριν. Άσε με να το διορθώσω."
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
                expertConfidence = 0.86f,
                adaptiveNpcLine = "Εντάξει... θα πάρω μια ανάσα. Νομίζω μπορούμε να το συζητήσουμε πιο ήρεμα."
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
                expertConfidence = 0.84f,
                adaptiveNpcLine = "Καλά... θα σου πω κάτι παραπάνω, αν και δεν ήθελα να το αναφέρω αρχικά."
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
                expertConfidence = 0.80f,
                adaptiveNpcLine = "Ναι, νομίζω ήρθε η ώρα να προχωρήσουμε σε αυτό."
            },

            new CaseData
            {
                caseId = "CASE-COOPERATIVE-INCOMPLETE-CLARIFY",
                situation = "NPC is cooperative but a key detail in their answer still seems missing.",
                npcState = "cooperative-but-incomplete",
                contextTags = new[] { "partial-cooperation", "hidden-detail", "clarification" },
                recommendedAction = PlayerActionType.Probe,
                rationale = "Clarifying is effective when the NPC is already cooperative and only a specific missing detail remains unexplored.",
                alternatives = "Stabilize keeps the NPC comfortable but leaves the detail hidden. Confront risks unsettling an already-cooperative NPC. Push Forward skips the missing detail entirely.",
                expectedOutcome = "The NPC adds the missing detail and the situation becomes much clearer.",
                expertConfidence = 0.83f,
                adaptiveNpcLine = "Α, σωστά -- ξέχασα να αναφέρω κάτι σημαντικό. Για να δούμε..."
            },

            new CaseData
            {
                caseId = "CASE-CONFIDENT-UNCERTAIN-CLARIFY",
                situation = "NPC repeats a confident-sounding claim, but the underlying information is still uncertain.",
                npcState = "confident-but-uncertain",
                contextTags = new[] { "premature-decision", "risk", "uncertainty" },
                recommendedAction = PlayerActionType.Probe,
                rationale = "Clarifying is effective when a confident claim is masking real uncertainty that needs to be examined before acting.",
                alternatives = "Stabilize keeps things calm but leaves the uncertainty unexamined. Confront can extract some information but adds tension. Push Forward risks a decision built on shaky information.",
                expectedOutcome = "The NPC slows down and provides additional context before any decision is made.",
                expertConfidence = 0.80f,
                adaptiveNpcLine = "Βασικά... δεν είμαι τόσο σίγουρος όσο ακούστηκα πριν. Άσε με να το ξανασκεφτώ."
            },

            new CaseData
            {
                caseId = "CASE-ANGRY-STABILIZE",
                situation = "NPC reacts strongly after feeling misunderstood and the interaction begins to escalate emotionally.",
                npcState = "angry",
                contextTags = new[] { "escalation", "misunderstanding", "emotional-intensity" },
                recommendedAction = PlayerActionType.Support,
                rationale = "Stabilizing is effective when strong emotion is driving the interaction and more pressure would only escalate it further.",
                alternatives = "Clarify can be read as more pressure. Confront is likely to escalate the reaction further. Push Forward ignores the NPC's emotional state entirely.",
                expectedOutcome = "The NPC feels acknowledged and the emotional intensity begins to drop.",
                expertConfidence = 0.87f,
                adaptiveNpcLine = "Συγγνώμη, αντέδρασα απότομα. Απλώς ένιωσα ότι δεν με καταλάβαινες."
            },

            new CaseData
            {
                caseId = "CASE-READY-PUSH-FORWARD",
                situation = "NPC has clarified the main issue and is ready for a concrete next step.",
                npcState = "ready",
                contextTags = new[] { "decision-point", "resolution", "high-clarity" },
                recommendedAction = PlayerActionType.Commit,
                rationale = "Push Forward is effective once the NPC is ready and clarity is already high -- delaying at this point reduces momentum without adding value.",
                alternatives = "Clarify or Stabilize delay unnecessarily once the NPC is already ready. Confront introduces friction at a point where none is needed.",
                expectedOutcome = "The player commits at the right moment and the interaction reaches a clear resolution.",
                expertConfidence = 0.85f,
                adaptiveNpcLine = "Ναι, είμαστε έτοιμοι. Ας προχωρήσουμε."
            },

            // ---------------------------------------------------------------
            // Cases derived from expert questionnaires (Κοινωνικός Λειτουργός,
            // Ψυχολόγος) collected for case library validation. Each is based
            // on a real professional incident, generalised to fit the game's
            // Probe / Support / Challenge / Commit action model.
            // ---------------------------------------------------------------

            new CaseData
            {
                caseId = "CASE-VAGUE-INFO-GRADUAL-PROBE",
                situation = "NPC gives vague, general answers about a difficult personal situation. Specific details are missing, but there is no strong resistance yet -- just hesitation.",
                npcState = "guarded",
                contextTags = new[] { "vague-answers", "trust-building", "low-pressure" },
                recommendedAction = PlayerActionType.Probe,
                rationale = "Persisting with specific, gentle clarifying questions -- rather than jumping to a decision or a stricter stance -- respects the NPC's pace while gradually closing the information gap.",
                alternatives = "Referring elsewhere or taking a stricter stance too early risks the NPC withdrawing before trust is established. Waiting silently without any clarifying question may never close the information gap.",
                expectedOutcome = "The NPC gradually feels safe enough to share more specific details and cooperates further in identifying next steps.",
                expertConfidence = 0.85f,
                adaptiveNpcLine = "Δεν το είχα πει σε κανέναν έτσι πριν... αλλά ναι, υπάρχει κάτι παραπάνω."
            },

            new CaseData
            {
                caseId = "CASE-AMBIGUOUS-INFO-GENTLE-PROBE",
                situation = "NPC's account contains ambiguous or seemingly contradictory statements, but the NPC has shown no outright hostility -- the contradictions may simply reflect confusion or distress rather than deception.",
                npcState = "ambiguous-info",
                contextTags = new[] { "ambiguous-statements", "contradictions", "fragile-trust" },
                recommendedAction = PlayerActionType.Probe,
                rationale = "Ambiguous but non-hostile contradictions are resolved more effectively with gentle clarifying questions than with direct confrontation, especially when trust is not yet firmly established.",
                alternatives = "Directly confronting the contradictions (Challenge) can make the NPC defensive and only partially cooperative, requiring extra time afterward to rebuild trust -- even if some information is eventually obtained.",
                expectedOutcome = "The NPC clarifies the ambiguous points without becoming defensive, preserving the working relationship.",
                expertConfidence = 0.78f,
                adaptiveNpcLine = "Συγγνώμη, δεν το εξήγησα καλά πριν. Άσε με να το ξαναπώ πιο καθαρά."
            },

            new CaseData
            {
                caseId = "CASE-PREMATURE-COMMIT-WITHDRAWAL",
                situation = "NPC is facing an urgent, serious need, but has not yet had a proper assessment of preferences, readiness, or pace.",
                npcState = "unready",
                contextTags = new[] { "urgent-need", "insufficient-assessment", "time-pressure" },
                recommendedAction = PlayerActionType.Probe,
                rationale = "Even when a situation appears urgent, checking the NPC's readiness and pace first -- before proposing a firm plan -- preserves engagement and avoids the plan feeling imposed.",
                alternatives = "Committing immediately to a fast, decisive plan (Commit) can feel imposed rather than collaborative, causing the NPC to feel pressured and to withdraw from the process, even though the intention was to help quickly.",
                expectedOutcome = "By checking in first, the NPC's true priorities and pace are identified before any plan is proposed, reducing the risk of withdrawal.",
                expertConfidence = 0.80f,
                adaptiveNpcLine = "Δώσε μου λίγο χρόνο να το σκεφτώ πρώτα, πριν προχωρήσουμε σε κάτι."
            },

            new CaseData
            {
                caseId = "CASE-STAGNANT-INSIGHT-CHALLENGE",
                situation = "NPC has gone through extensive supportive engagement without reaching any new insight or change, appearing comfortable remaining in the same pattern.",
                npcState = "stagnant",
                contextTags = new[] { "plateaued-progress", "comfortable-pattern", "needs-insight" },
                recommendedAction = PlayerActionType.Challenge,
                rationale = "When purely supportive engagement has been tried extensively without progress, a direct challenge can prompt the NPC to confront the situation and their own responsibility, opening the door to real change.",
                alternatives = "Continuing with more support alone risks reinforcing the same stagnant pattern indefinitely. Referring elsewhere risks losing the rapport already built without giving challenge a chance to work.",
                expectedOutcome = "The NPC is initially taken aback, having expected more validation, but begins to adopt a different perspective on the situation.",
                expertConfidence = 0.82f,
                adaptiveNpcLine = "Δεν το περίμενα αυτό από σένα... αλλά ίσως έχεις δίκιο."
            },

            new CaseData
            {
                caseId = "CASE-COMPETING-NEEDS-SUPPORT-FIRST",
                situation = "External pressure (institutional deadlines or requirements) pushes toward deep exploration of a difficult topic, even though the NPC has other, more urgent unmet needs at that moment.",
                npcState = "overwhelmed-competing-needs",
                contextTags = new[] { "competing-needs", "external-pressure", "incomplete-readiness" },
                recommendedAction = PlayerActionType.Support,
                rationale = "When the NPC has more pressing, unmet needs, addressing those first -- rather than pushing ahead with deep exploration to satisfy external requirements -- respects the NPC's actual priorities and readiness.",
                alternatives = "Pursuing deep clarifying questions (Probe) to meet institutional demands, while urgent needs remain unaddressed, can produce some information but leaves the NPC feeling pressured and inadequately prioritised.",
                expectedOutcome = "The NPC feels their immediate priorities are acknowledged first, making them more receptive to deeper exploration afterward.",
                expertConfidence = 0.78f,
                adaptiveNpcLine = "Πρώτα... μπορούμε να δούμε αυτό που με απασχολεί τώρα; Το άλλο μπορεί να περιμένει λίγο."
            },

            new CaseData
            {
                caseId = "CASE-VALIDATE-PEER-INSIGHT",
                situation = "NPC shares a meaningful personal insight -- that mutual support from peers or community helped them heal -- but this insight risks being dismissed in favour of an externally imposed plan.",
                npcState = "sharing-insight",
                contextTags = new[] { "positive-disclosure", "peer-support", "autonomy" },
                recommendedAction = PlayerActionType.Support,
                rationale = "Validating and building on an insight the NPC has reached themselves respects their autonomy and reinforces something that has already proven helpful to them.",
                alternatives = "Challenging the value of what the NPC found helpful and unilaterally imposing a different plan (Commit without collaboration) can feel like their own judgment and autonomy are being overridden, leading to disengagement or withdrawal.",
                expectedOutcome = "The NPC feels heard and understood, remaining engaged and open to continuing the process collaboratively.",
                expertConfidence = 0.84f,
                adaptiveNpcLine = "Ναι... αυτό πραγματικά με βοήθησε. Χαίρομαι που δεν το προσπερνάς."
            }
        };
    }

    /// <summary>
    /// Υπολογίζει το τελικό influence change για μια ενέργεια στο Adaptive
    /// mode. Οι κανόνες 1-4 παρακάτω αποτελούν μια ΗΠΙΑ, σιωπηλή μορφή
    /// Dynamic Difficulty Adjustment (rubber-banding): δεν αλλάζουν ΤΙ
    /// βλέπει ο παίκτης (τα rounds/περιεχόμενο μένουν ίδια), απλά
    /// τσιμπολογάνε ελαφρώς το influence score ώστε η εμπειρία να μη
    /// νιώθεται ούτε υπερβολικά τιμωρητική σε δύσκολο παίκτη, ούτε
    /// υπερβολικά "δωρεάν" σε παίκτη που παίρνει βιαστικές αποφάσεις.
    /// Το ρητό, ορατό DDA (reordering rounds βάσει επίδοσης) γίνεται
    /// ξεχωριστά, στο GameManager.ReorderRemainingRoundsForAdaptiveDDA().
    ///
    /// ΣΗΜΕΙΩΣΗ (Revise): χρησιμοποιεί lastSecondCase/lastSecondScore, που
    /// ενημερώνονται από την GetAdaptiveResponse. Ο caller (AIManager.EvaluateAction)
    /// καλεί πάντα πρώτα GetAdaptiveResponse και μετά αυτή τη μέθοδο στο ίδιο
    /// round, άρα τα δεδομένα είναι πάντα φρέσκα.
    /// </summary>
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
            return ApplyCaseConfidence(finalChange, selectedAction, matchedCase, matchScore, lastSecondCase, lastSecondScore);

        float riskPreference = tracker.GetRiskPreference();
        int wrongActions = tracker.wrongActions;

        // Rule 1 [Rubber-banding -- δυσκολία↓]: If player struggles, reduce negative penalties slightly.
        if (baseInfluenceChange < 0 && wrongActions >= 2)
        {
            finalChange += 1;
        }

        // Rule 2 [Rubber-banding -- ανταμοιβή ικανού παίκτη]: If player makes a strong choice quickly, reward decisiveness slightly.
        if (isCorrectAction && baseInfluenceChange > 0 && reactionTime <= fastResponseThreshold)
        {
            finalChange += 1;
        }

        // Rule 3 [Rubber-banding -- αποθάρρυνση απερισκεψίας]: If player makes a bad choice too quickly, punish impulsive mistakes slightly.
        if (!isCorrectAction &&
            baseInfluenceChange < 0 &&
            reactionTime <= fastResponseThreshold &&
            tracker.totalActions > 0)
        {
            finalChange -= 1;
        }

        // Rule 4 [Rubber-banding -- δυσκολία↑]: If player prefers risky play and fails with Commit, punish more.
        if (!isCorrectAction && selectedAction == PlayerActionType.Commit && riskPreference >= 0.40f)
        {
            finalChange -= 2;
        }

        return ApplyCaseConfidence(finalChange, selectedAction, matchedCase, matchScore, lastSecondCase, lastSecondScore);
    }

    public string GetAdaptiveResponse(
        int roundNumber,
        RoundData round,
        PlayerActionType action,
        bool isCorrectAction,
        PlayerBehaviourTracker tracker)
    {
        lastMatchedCase = FindBestCase(round, action, out lastMatchScore, out lastSecondCase, out lastSecondScore);

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

    /// <summary>
    /// Revise-related getters: εκθέτουν το 2ο-καλύτερο matched case, ώστε το
    /// AIManager να μπορεί προαιρετικά να τα καταγράψει στο dataset (π.χ.
    /// ambiguityGap = lastMatchScore - lastSecondScore) για ανάλυση.
    /// </summary>
    public CaseData GetLastSecondCase()
    {
        return lastSecondCase;
    }

    public float GetLastSecondScore()
    {
        return lastSecondScore;
    }

    /// <summary>
    /// Επιστρέφει την player-facing ατάκα του τελευταίου matched case, με τρεις
    /// πιθανές εκβάσεις, με σειρά προτεραιότητας:
    /// 1) Αν τα contextTags του round έχουν ήδη "λυθεί" νωρίτερα στο ίδιο block
    ///    (βλ. WasAlreadyResolved/MarkResolved), επιστρέφεται μια ειδική ατάκα
    ///    "μνήμης" -- ορατό, αισθητό σήμα ότι το Adaptive mode λαμβάνει υπόψη
    ///    το ιστορικό του session, σε αντίθεση με το Static mode.
    /// 2) Αλλιώς, αν το matchScore είναι αρκετά υψηλό (>= dialogueOverrideThreshold)
    ///    ΚΑΙ το case έχει ορισμένο adaptiveNpcLine, επιστρέφεται αυτό.
    /// 3) Σε κάθε άλλη περίπτωση επιστρέφεται null, ώστε ο caller (GameManager)
    ///    να κάνει fallback στο κανονικό RoundData.outcomes[].npcResponse.
    /// </summary>
    /// <param name="round">Το τρέχον round, χρειάζεται για τα contextTags του.</param>
    public string GetAdaptiveNpcLine(RoundData round)
    {
        if (lastMatchedCase == null)
            return null;

        if (lastMatchScore < dialogueOverrideThreshold)
            return null;

        if (round != null && WasAlreadyResolved(round.contextTags, round.difficulty))
        {
            // Ορατό, αισθητό σήμα "μνήμης" -- διαφορετικό από την κανονική
            // adaptiveNpcLine, ώστε ο παίκτης να το αντιληφθεί ως διακριτή
            // αντίδραση και όχι απλά ως άλλη τυχαία ατάκα.
            return "Το είχαμε ήδη ξεκαθαρίσει αυτό, δεν είναι έτσι;";
        }

        if (string.IsNullOrWhiteSpace(lastMatchedCase.adaptiveNpcLine))
            return null;

        return lastMatchedCase.adaptiveNpcLine;
    }

    /// <summary>
    /// Βοηθητική μέθοδος για logging/dataset: δείχνει αν το τρέχον round
    /// θεωρήθηκε ήδη "λυμένο" από προηγούμενο γύρο του ίδιου block, ώστε να
    /// μπορεί να καταγραφεί σαν ξεχωριστή στήλη στο CSV (π.χ. wasAlreadyResolved)
    /// και να αναλυθεί μετά πόσο συχνά συνέβη.
    /// </summary>
    public bool GetLastWasAlreadyResolved(RoundData round)
    {
        return round != null && WasAlreadyResolved(round.contextTags, round.difficulty);
    }

    public string GetBehaviourLabel(
        PlayerBehaviourTracker tracker,
        PlayerActionType action,
        bool isCorrectAction,
        float reactionTime
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

        // Logging-only label: ο παίκτης απάντησε αισθητά πιο αργά από τον
        // δικό του μέσο όρο. Το slowResponseThreshold λειτουργεί εδώ ως
        // "πάτωμα" (floor) -- ένας παίκτης με πολύ γρήγορο μέσο όρο δεν
        // χαρακτηρίζεται "deliberate" για μια ελαφρώς πιο αργή αλλά ακόμα
        // φυσιολογική κίνηση. ΔΕΝ επηρεάζει καθόλου τη βαθμολογία/influence,
        // μόνο το behaviourLabel για logging/ανάλυση.
        float deliberateThreshold = Mathf.Max(tracker.averageReactionTime * 1.5f, slowResponseThreshold);
        if (reactionTime >= deliberateThreshold)
        {
            return "deliberate";
        }

        return "neutral";
    }

    // ---------- Revise βήμα προετοιμασίας: FindBestCase επιστρέφει και το 2ο καλύτερο ----------
    private CaseData FindBestCase(RoundData round, PlayerActionType action, out float bestScore, out CaseData secondCase, out float secondScore)
    {
        bestScore = 0f;
        secondScore = 0f;
        CaseData bestCase = null;
        secondCase = null;

        if (round == null || caseLibrary == null)
            return null;

        foreach (CaseData candidate in caseLibrary)
        {
            if (candidate == null)
                continue;

            float score = ScoreCase(candidate, round, action);

            if (score > bestScore)
            {
                // ό,τι ήταν πρώτο πέφτει στη δεύτερη θέση
                secondScore = bestScore;
                secondCase = bestCase;

                bestScore = score;
                bestCase = candidate;
            }
            else if (score > secondScore)
            {
                secondScore = score;
                secondCase = candidate;
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

    /// <summary>
    /// Revise: εφαρμόζει βαθμιαία εμπιστοσύνη γύρω από το threshold αντί για
    /// hard cutoff, και μειώνει την επιρροή όταν το 2ο-καλύτερο case προτείνει
    /// διαφορετική ενέργεια και είναι πολύ κοντά σε score (ambiguity penalty).
    /// </summary>
    private int ApplyCaseConfidence(int influenceChange, PlayerActionType selectedAction, CaseData matchedCase, float matchScore, CaseData secondCase, float secondScore)
    {
        if (matchedCase == null)
            return influenceChange;

        const float fullConfidenceThreshold = 0.60f;
        const float minConfidenceThreshold = 0.45f;

        // --- Revise βήμα Α: graded confidence αντί για hard cutoff ---
        float confidenceFactor;
        if (matchScore >= fullConfidenceThreshold)
            confidenceFactor = 1f;
        else if (matchScore <= minConfidenceThreshold)
            confidenceFactor = 0f;
        else
            confidenceFactor = (matchScore - minConfidenceThreshold) /
                                (fullConfidenceThreshold - minConfidenceThreshold);

        if (confidenceFactor <= 0f)
            return influenceChange;

        // --- Revise βήμα Β: blend με το 2ο καλύτερο case, αν είναι πολύ κοντά ---
        // (π.χ. bestScore=0.62, secondScore=0.58 -> δεν είμαστε τόσο σίγουροι
        // ποιο case "ταιριάζει πραγματικά", άρα μετριάζουμε την επιρροή)
        float ambiguityPenalty = 0f;
        if (secondCase != null && secondCase.recommendedAction != matchedCase.recommendedAction)
        {
            float gap = matchScore - secondScore;
            if (gap < 0.10f)
                ambiguityPenalty = (0.10f - gap); // 0..0.10
        }

        float effectiveConfidence = Mathf.Clamp01(confidenceFactor - ambiguityPenalty);

        int adjustedBonus = Mathf.RoundToInt(matchedCase.expertConfidence * 2f * effectiveConfidence);

        if (selectedAction == matchedCase.recommendedAction && influenceChange > 0)
            return influenceChange + adjustedBonus;

        if (selectedAction != matchedCase.recommendedAction && influenceChange < 0)
            return influenceChange - Mathf.RoundToInt(1 * effectiveConfidence);

        return influenceChange;
    }

    /// <summary>
    /// Καταγράφει ότι τα contextTags ενός round έχουν "λυθεί" ικανοποιητικά
    /// μέσα σε αυτό το block (Best ή Acceptable outcome). Καλείται ΜΟΝΟ από
    /// το Adaptive mode (βλ. AIManager.MarkResolvedIfAdaptive) -- το Static
    /// mode δεν έχει καθόλου πρόσβαση σε αυτή τη μνήμη, ώστε η διαφορά
    /// "θυμάται/δεν θυμάται" να παραμένει καθαρά ελεγχόμενη μεταβλητή.
    /// Δεν επηρεάζει ποτέ ποια rounds εμφανίζονται ή τη σειρά τους -- μόνο
    /// το ΠΟΙΟ κείμενο/ατάκα θα επιλέξει το GetAdaptiveNpcLine παρακάτω.
    /// </summary>
    /// <param name="contextTags">Τα contextTags του round που μόλις ολοκληρώθηκε.</param>
    /// <param name="quality">Η ποιότητα του outcome που πέτυχε ο παίκτης σε αυτό το round.</param>
    /// <param name="difficulty">Παραμένει στην υπογραφή για συμβατότητα με τον caller· δεν επηρεάζει τον έλεγχο ομοιότητας.</param>
    public void MarkResolved(string[] contextTags, ActionOutcomeQuality quality, DifficultyLevel difficulty)
    {
        bool isGoodOutcome = quality == ActionOutcomeQuality.Best || quality == ActionOutcomeQuality.Acceptable;
        if (!isGoodOutcome || contextTags == null)
            return;

        resolvedContextTags.Add(new ResolvedRoundMemory
        {
            Tags = contextTags,
            Difficulty = difficulty
        });
    }

    /// <summary>
    /// Ελέγχει αν ΈΝΑ τουλάχιστον από τα contextTags του τρέχοντος round έχει
    /// ήδη καταγραφεί ως "λυμένο" νωρίτερα στο ίδιο block (βλ. MarkResolved).
    /// Χρησιμοποιείται από το GetAdaptiveNpcLine για να αποφασίσει αν πρέπει
    /// να εμφανιστεί η ειδική ατάκα "memory" αντί για την κανονική
    /// adaptiveNpcLine του matched case.
    /// </summary>
    /// <summary>
    /// Ελέγχει αν το τρέχον round είναι πραγματικά ΟΜΟΙΟ με κάποιο
    /// συγκεκριμένο round που "λύθηκε" νωρίτερα στο ίδιο block: βασίζεται
    /// ΜΟΝΟ στα contextTags και απαιτεί τουλάχιστον
    /// <see cref="MinSharedTagsForMemory"/> κοινά tags με εκείνο το
    /// προηγούμενο round. Έτσι δεν σκανδαλίζει απλά επειδή δύο εντελώς
    /// διαφορετικά σκηνικά μοιράζονται 1 γενικό tag (π.χ. "stability").
    /// </summary>
    private bool WasAlreadyResolved(string[] contextTags, DifficultyLevel difficulty)
    {
        if (contextTags == null)
            return false;

        HashSet<string> currentTags = new HashSet<string>();
        foreach (string tag in contextTags)
        {
            if (!string.IsNullOrWhiteSpace(tag))
                currentTags.Add(tag.Trim().ToLowerInvariant());
        }

        foreach (ResolvedRoundMemory memory in resolvedContextTags)
        {
            int sharedCount = 0;
            foreach (string tag in memory.Tags)
            {
                if (!string.IsNullOrWhiteSpace(tag) && currentTags.Contains(tag.Trim().ToLowerInvariant()))
                    sharedCount++;
            }

            if (sharedCount >= MinSharedTagsForMemory)
                return true;
        }

        return false;
    }
}