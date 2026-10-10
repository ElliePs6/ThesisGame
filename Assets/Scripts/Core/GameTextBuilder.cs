public static class GameTextBuilder
{
    public static string BuildPlayerFeedback(
        PlayerActionType selectedAction,
        ActionOutcome selectedOutcome,
        bool isCorrectAction)
    {
        string actionName = GetActionDisplayName(selectedAction);

        if (selectedOutcome == null)
        {
            return isCorrectAction
                ? $"Η {actionName} ήταν κατάλληλη για αυτή την κατάσταση."
                : $"Η {actionName} δεν ταίριαζε στις ανάγκες αυτής της κατάστασης.";
        }

        switch (selectedOutcome.quality)
        {
            case ActionOutcomeQuality.Best:
                return $"Η {actionName} ήταν η πιο δυνατή επιλογή για αυτή την κατάσταση.";

            case ActionOutcomeQuality.Acceptable:
                return $"Η {actionName} ήταν μια λογική επιλογή, αλλά μια άλλη προσέγγιση θα μπορούσε να αντιμετωπίσει την κατάσταση πιο άμεσα.";

            case ActionOutcomeQuality.Bad:
                return $"Η {actionName} δεν ταίριαζε καλά στην κατάσταση και μείωσε την αποτελεσματικότητα της συζήτησης.";

            case ActionOutcomeQuality.VeryBad:
                return $"Η {actionName} λειτούργησε αντίθετα στις ανάγκες της κατάστασης και έκανε τη συζήτηση πιο δύσκολη.";

            default:
                return isCorrectAction
                    ? $"Η {actionName} ταίριαζε στις ανάγκες της κατάστασης."
                    : $"Η {actionName} δεν ταίριαζε στις ανάγκες της κατάστασης.";
        }
    }

    public static string GetActionDisplayName(PlayerActionType action)
    {
        switch (action)
        {
            case PlayerActionType.Probe:
                return "Διευκρίνιση";

            case PlayerActionType.Challenge:
                return "Αντιπαράθεση";

            case PlayerActionType.Support:
                return "Σταθεροποίηση";

            case PlayerActionType.Commit:
                return "Τοποθέτηση";

            default:
                return action.ToString();
        }
    }

    public static string GetAuthorityMessage(
        ActionOutcome selectedOutcome,
        bool isCorrectAction)
    {
        if (selectedOutcome == null)
        {
            return isCorrectAction
                ? "Σωστή επιλογή"
                : "Λανθασμένη επιλογή";
        }

        switch (selectedOutcome.quality)
        {
            case ActionOutcomeQuality.Best:
                return "Σωστή επιλογή";

            case ActionOutcomeQuality.Acceptable:
                return "Αποδεκτή επιλογή";

            case ActionOutcomeQuality.Bad:
                return "Κακή επιλογή";

            case ActionOutcomeQuality.VeryBad:
                return "Κρίσιμο λάθος";

            default:
                return isCorrectAction
                    ? "Σωστή επιλογή"
                    : "Λανθασμένη επιλογή";
        }
    }

    public static string GetInfluenceChangeMessage(int influenceChange)
    {
        string formattedChange = influenceChange >= 0
            ? $"+{influenceChange}"
            : influenceChange.ToString();

        return $"Επιρροή {formattedChange}";
    }
}