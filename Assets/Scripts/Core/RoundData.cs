using System;
using UnityEngine;

public enum CaseType
{
    InformationGap,
    Defensive,
    Contradiction,
    Escalated,
    HiddenDetail,
    DecisionPoint
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public enum CommitTiming
{
    None,
    Early,
    Middle,
    Late
}

public enum RoundPosition
{
    Opening,
    Development,
    Closing,
    Any
}

/// <summary>
/// Καθορίζει σε ποιο από τα δύο ισοδύναμα υποσύνολα rounds (Α/Β) ανήκει
/// κάθε round -- χρησιμοποιείται για το counterbalanced within-subjects
/// σχέδιο μελέτης (κάθε participant παίζει κάθε AI mode με διαφορετικό
/// υποσύνολο rounds, ώστε να μην ξαναδεί τις ίδιες σωστές απαντήσεις).
/// </summary>
public enum RoundSubsetGroup
{
    A,
    B
}

[Serializable]
public class RoundData
{
    [Header("Round Identity")]
    public string scenarioId;

    [TextArea(2, 4)]
    public string statement;

    [Header("Round Metadata")]
    public CaseType caseType = CaseType.InformationGap;
    public DifficultyLevel difficulty = DifficultyLevel.Easy;
    public CommitTiming commitTiming = CommitTiming.None;
    public RoundPosition roundPosition = RoundPosition.Any;

    [Header("Study Design")]
    [Tooltip("Σε ποιο υποσύνολο (Α/Β) ανήκει αυτό το round, για το counterbalanced within-subjects design.")]
    public RoundSubsetGroup subsetGroup = RoundSubsetGroup.A;

    [Header("NPC / Context")]
    public string npcState;

    public string[] contextTags;

    [Header("Evaluation")]
    public PlayerActionType preferredAction;

    public ActionOutcome[] outcomes;

    public ActionOutcome GetOutcome(PlayerActionType selectedAction)
    {
        if (outcomes == null)
            return null;

        foreach (ActionOutcome outcome in outcomes)
        {
            if (outcome != null && outcome.action == selectedAction)
            {
                return outcome;
            }
        }

        return null;
    }
}