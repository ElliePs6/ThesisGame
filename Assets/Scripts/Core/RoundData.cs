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
