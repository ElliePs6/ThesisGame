using System;
using System.Collections.Generic;
using UnityEngine;

public static class SessionBuilder
{
    public static RoundData[] BuildBalancedSession(RoundData[] roundPool, int targetRoundCount)
    {
        List<RoundData> validRounds = GetValidRounds(roundPool);

        if (validRounds.Count == 0)
        {
            Debug.LogWarning("SessionBuilder: No valid rounds found.");
            return new RoundData[0];
        }

        int targetCount = Mathf.Clamp(targetRoundCount, 1, validRounds.Count);

        if (validRounds.Count <= targetCount)
        {
            Debug.Log($"SessionBuilder: Pool has {validRounds.Count} rounds. Using all available rounds.");
            return validRounds.ToArray();
        }

        List<RoundData> selectedRounds = new List<RoundData>();

        AddOpeningRound(validRounds, selectedRounds, targetCount);
        AddOneRoundPerCaseType(validRounds, selectedRounds, targetCount);
        FillRemainingBalancedByDifficulty(validRounds, selectedRounds, targetCount);
        MoveClosingRoundToEnd(selectedRounds);

        Debug.Log($"SessionBuilder: Built balanced session with {selectedRounds.Count} rounds from a pool of {validRounds.Count}.");

        return selectedRounds.ToArray();
    }

    private static List<RoundData> GetValidRounds(RoundData[] roundPool)
    {
        List<RoundData> validRounds = new List<RoundData>();

        if (roundPool == null)
            return validRounds;

        foreach (RoundData round in roundPool)
        {
            if (round == null)
                continue;

            if (string.IsNullOrWhiteSpace(round.statement))
                continue;

            if (round.outcomes == null || round.outcomes.Length == 0)
                continue;

            validRounds.Add(round);
        }

        return validRounds;
    }

    private static void AddOpeningRound(List<RoundData> validRounds, List<RoundData> selectedRounds, int targetCount)
    {
        if (selectedRounds.Count >= targetCount)
            return;

        RoundData openingRound = FindFirstUnusedRound(
            validRounds,
            selectedRounds,
            round => round.roundPosition == RoundPosition.Opening
        );

        if (openingRound != null)
        {
            selectedRounds.Add(openingRound);
        }
    }

    private static void AddOneRoundPerCaseType(List<RoundData> validRounds, List<RoundData> selectedRounds, int targetCount)
    {
        CaseType[] caseTypes = (CaseType[])Enum.GetValues(typeof(CaseType));

        foreach (CaseType caseType in caseTypes)
        {
            if (selectedRounds.Count >= targetCount)
                return;

            bool alreadyHasCaseType = selectedRounds.Exists(round => round.caseType == caseType);

            if (alreadyHasCaseType)
                continue;

            RoundData matchingRound = FindFirstUnusedRound(
                validRounds,
                selectedRounds,
                round => round.caseType == caseType
            );

            if (matchingRound != null)
            {
                selectedRounds.Add(matchingRound);
            }
        }
    }

    private static void FillRemainingBalancedByDifficulty(List<RoundData> validRounds, List<RoundData> selectedRounds, int targetCount)
    {
        while (selectedRounds.Count < targetCount)
        {
            RoundData nextRound = FindBestUnusedRoundByDifficultyBalance(validRounds, selectedRounds);

            if (nextRound == null)
                break;

            selectedRounds.Add(nextRound);
        }
    }

    private static RoundData FindBestUnusedRoundByDifficultyBalance(List<RoundData> validRounds, List<RoundData> selectedRounds)
    {
        RoundData bestRound = null;
        int bestDifficultyCount = int.MaxValue;
        int bestCaseTypeCount = int.MaxValue;

        foreach (RoundData candidate in validRounds)
        {
            if (selectedRounds.Contains(candidate))
                continue;

            int difficultyCount = selectedRounds.FindAll(round => round.difficulty == candidate.difficulty).Count;
            int caseTypeCount = selectedRounds.FindAll(round => round.caseType == candidate.caseType).Count;

            if (difficultyCount < bestDifficultyCount)
            {
                bestRound = candidate;
                bestDifficultyCount = difficultyCount;
                bestCaseTypeCount = caseTypeCount;
                continue;
            }

            if (difficultyCount == bestDifficultyCount && caseTypeCount < bestCaseTypeCount)
            {
                bestRound = candidate;
                bestCaseTypeCount = caseTypeCount;
            }
        }

        return bestRound;
    }

    private static RoundData FindFirstUnusedRound(
        List<RoundData> validRounds,
        List<RoundData> selectedRounds,
        Predicate<RoundData> condition
    )
    {
        foreach (RoundData round in validRounds)
        {
            if (selectedRounds.Contains(round))
                continue;

            if (condition(round))
                return round;
        }

        return null;
    }

    private static void MoveClosingRoundToEnd(List<RoundData> selectedRounds)
    {
        int closingIndex = selectedRounds.FindIndex(round => round.roundPosition == RoundPosition.Closing);

        if (closingIndex < 0)
            return;

        RoundData closingRound = selectedRounds[closingIndex];
        selectedRounds.RemoveAt(closingIndex);
        selectedRounds.Add(closingRound);
    }
}
