using UnityEngine;

public class StaticAI : MonoBehaviour
{

    public AIEvaluationResult Evaluate(
        int roundNumber,
        PlayerActionType action,
        bool isCorrectAction,
        int baseInfluenceChange
    )
    {
        return new AIEvaluationResult
        {
            responseText = GetResponse(roundNumber, action, isCorrectAction),
            influenceChange = baseInfluenceChange,
            matchedCaseId = $"STATIC-R{roundNumber:00}",
            matchScore = 1f
        };
    }
    public string GetResponse(int roundNumber, PlayerActionType action, bool isCorrectAction)
    {
        switch (roundNumber)
        {
            case 1:
                return GetRound1Response(action, isCorrectAction);

            case 2:
                return GetRound2Response(action, isCorrectAction);

            case 3:
                return GetRound3Response(action, isCorrectAction);

            case 4:
                return GetRound4Response(action, isCorrectAction);

            case 5:
                return GetRound5Response(action, isCorrectAction);

            case 6:
                return GetRound6Response(action, isCorrectAction);

            case 7:
                return GetRound7Response(action, isCorrectAction);

            case 8:
                return GetRound8Response(action, isCorrectAction);

            case 9:
                return GetRound9Response(action, isCorrectAction);

            case 10:
                return GetRound10Response(action, isCorrectAction);

            default:
                return "Static AI: No scripted response available for this round.";
        }
    }

    private string GetRound1Response(PlayerActionType action, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            return "Static AI: The Primary NPC hesitates and reveals a little more information.";
        }

        switch (action)
        {
            case PlayerActionType.Challenge:
                return "Static AI: The Primary NPC withdraws and becomes less cooperative.";
            case PlayerActionType.Support:
                return "Static AI: The Primary NPC relaxes, but avoids the missing details.";
            case PlayerActionType.Commit:
                return "Static AI: The Primary NPC reacts poorly to the high-pressure move.";
            default:
                return "Static AI: The interaction produces little useful progress.";
        }
    }

    private string GetRound2Response(PlayerActionType action, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            return "Static AI: The Primary NPC calms down and becomes more open to the interaction.";
        }

        switch (action)
        {
            case PlayerActionType.Probe:
                return "Static AI: The Primary NPC feels pressured and stays guarded.";
            case PlayerActionType.Challenge:
                return "Static AI: The Primary NPC becomes more defensive after being challenged.";
            case PlayerActionType.Commit:
                return "Static AI: The aggressive move increases tension in the scene.";
            default:
                return "Static AI: The response remains mostly unchanged.";
        }
    }

    private string GetRound3Response(PlayerActionType action, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            return "Static AI: The inconsistency is exposed and the situation becomes clearer.";
        }

        switch (action)
        {
            case PlayerActionType.Probe:
                return "Static AI: The Primary NPC gives more detail, but the contradiction remains unresolved.";
            case PlayerActionType.Support:
                return "Static AI: The Primary NPC feels understood, but the inconsistency is left unaddressed.";
            case PlayerActionType.Commit:
                return "Static AI: The high-risk move creates pressure, but misses the key contradiction.";
            default:
                return "Static AI: The response fails to resolve the situation.";
        }
    }

    private string GetRound4Response(PlayerActionType action, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            return "Static AI: The Primary NPC regains enough composure to continue.";
        }

        switch (action)
        {
            case PlayerActionType.Probe:
                return "Static AI: The Primary NPC struggles with the added questioning.";
            case PlayerActionType.Challenge:
                return "Static AI: The direct challenge increases emotional pressure.";
            case PlayerActionType.Commit:
                return "Static AI: The decisive move feels premature and damages trust.";
            default:
                return "Static AI: The interaction remains unstable.";
        }
    }

    private string GetRound5Response(PlayerActionType action, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            return "Static AI: The Primary NPC gives a more specific answer and reveals the hidden detail.";
        }

        switch (action)
        {
            case PlayerActionType.Challenge:
                return "Static AI: The NPC becomes defensive before the missing detail is clarified.";
            case PlayerActionType.Support:
                return "Static AI: The NPC stays calm, but the important detail remains vague.";
            case PlayerActionType.Commit:
                return "Static AI: The decisive move skips over the information gap.";
            default:
                return "Static AI: The exchange produces limited progress.";
        }
    }

    private string GetRound6Response(PlayerActionType action, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            return "Static AI: The interaction moves toward a clear outcome.";
        }

        switch (action)
        {
            case PlayerActionType.Probe:
                return "Static AI: More questioning delays the decision point.";
            case PlayerActionType.Challenge:
                return "Static AI: The confrontation creates friction without resolving the firm position.";
            case PlayerActionType.Support:
                return "Static AI: The NPC feels acknowledged, but the scene still needs a decision.";
            default:
                return "Static AI: The interaction does not reach a clear resolution.";
        }
    }


    private string GetRound7Response(PlayerActionType action, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            return "Static AI: The Primary NPC adds the missing detail and the situation becomes clearer.";
        }

        switch (action)
        {
            case PlayerActionType.Support:
                return "Static AI: The NPC remains calm, but the missing detail is still not fully explored.";
            case PlayerActionType.Challenge:
                return "Static AI: The sudden pressure makes the NPC more defensive.";
            case PlayerActionType.Commit:
                return "Static AI: The player moves forward while some uncertainty remains.";
            default:
                return "Static AI: The exchange produces limited progress.";
        }
    }

    private string GetRound8Response(PlayerActionType action, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            return "Static AI: The NPC slows down and provides more context before a decision is made.";
        }

        switch (action)
        {
            case PlayerActionType.Support:
                return "Static AI: The NPC stays calm, but the uncertainty is not fully examined.";
            case PlayerActionType.Challenge:
                return "Static AI: The NPC defends the claim, creating tension but revealing some information.";
            case PlayerActionType.Commit:
                return "Static AI: The player commits too early and the decision path becomes weaker.";
            default:
                return "Static AI: The situation remains uncertain.";
        }
    }

    private string GetRound9Response(PlayerActionType action, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            return "Static AI: The NPC feels acknowledged and the emotional intensity begins to drop.";
        }

        switch (action)
        {
            case PlayerActionType.Probe:
                return "Static AI: The NPC experiences the question as pressure and becomes less willing to continue.";
            case PlayerActionType.Challenge:
                return "Static AI: The confrontation escalates the emotional reaction.";
            case PlayerActionType.Commit:
                return "Static AI: The player tries to force progress before the NPC is stable.";
            default:
                return "Static AI: The interaction remains emotionally unstable.";
        }
    }

    private string GetRound10Response(PlayerActionType action, bool isCorrectAction)
    {
        if (isCorrectAction)
        {
            return "Static AI: The player commits at the right moment and the interaction reaches a clear resolution.";
        }

        switch (action)
        {
            case PlayerActionType.Support:
                return "Static AI: The NPC remains stable, but the interaction still needs a concrete next step.";
            case PlayerActionType.Probe:
                return "Static AI: More questioning delays a situation that is already clear.";
            case PlayerActionType.Challenge:
                return "Static AI: The confrontation creates unnecessary friction near resolution.";
            default:
                return "Static AI: The interaction does not reach a clear resolution.";
        }
    }
}
