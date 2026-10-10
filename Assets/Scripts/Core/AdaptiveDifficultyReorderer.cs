public static class AdaptiveDifficultyReorderer
{
    /// <summary>
    /// Αναδιατάσσει ΜΟΝΟ στο Adaptive mode, ώστε το Static να παραμείνει
    /// καθαρό baseline σύγκρισης. Αναδιατάσσει ΜΟΝΟ τη ΣΕΙΡΑ των rounds
    /// που απομένουν μέσα στο ήδη σταθερό υποσύνολο (subset A/B) -- ΠΟΤΕ
    /// δεν αλλάζει ποια rounds εμφανίζονται, κρίσιμο για να μη χαλάσει το
    /// counterbalancing design. Δεν ενεργοποιείται πριν συμπληρωθούν
    /// minSampleSizeForDDA ενέργειες, ώστε να μην αντιδρά "νευρικά" σε
    /// ένα μόνο, μη αντιπροσωπευτικό γύρο. Καλή απόδοση -> τα πιο δύσκολα
    /// rounds "ανεβαίνουν" νωρίτερα (μεγαλύτερη πρόκληση). Δυσκολία -> τα
    /// πιο εύκολα "ανεβαίνουν" νωρίτερα (λίγη ανάσα). Μέτρια απόδοση -> η
    /// ήδη προγραμματισμένη σειρά μένει όπως είναι.
    ///
    /// Σημείωση: το sessionRounds array τροποποιείται in-place (τα arrays
    /// στη C# είναι reference types), οπότε ο caller βλέπει αμέσως την
    /// αναδιάταξη χωρίς να χρειάζεται να επιστραφεί τίποτα.
    /// </summary>
    public static void ReorderRemaining(
        AIMode currentAIMode,
        PlayerBehaviourTracker behaviourTracker,
        RoundData[] sessionRounds,
        int currentRoundIndex,
        GameLogger gameLogger)
    {
        if (currentAIMode != AIMode.Adaptive)
            return;

        if (behaviourTracker == null || sessionRounds == null)
            return;

        int remainingCount = sessionRounds.Length - currentRoundIndex;
        if (remainingCount < 2)
            return;

        // Ελάχιστο δείγμα πριν ενεργοποιηθεί καθόλου το DDA: το successRate
        // μετά από 1 μόνο γύρο είναι είτε 0% είτε 100%, μη αντιπροσωπευτικό.
        // Περιμένουμε τουλάχιστον 2 ολοκληρωμένες ενέργειες πριν αρχίσουμε
        // να αναδιατάσσουμε τη σειρά, ώστε το DDA να μη "τιναχτεί" βάσει
        // ενός μεμονωμένου γύρου.
        const int minSampleSizeForDDA = 2;
        if (behaviourTracker.totalActions < minSampleSizeForDDA)
            return;

        float successRate = behaviourTracker.GetSuccessRate();

        RoundData[] remaining = new RoundData[remainingCount];
        System.Array.Copy(sessionRounds, currentRoundIndex, remaining, 0, remainingCount);

        if (successRate >= 0.70f)
        {
            System.Array.Sort(remaining, (a, b) => DifficultyRank(b.difficulty).CompareTo(DifficultyRank(a.difficulty)));
        }
        else if (successRate <= 0.40f)
        {
            System.Array.Sort(remaining, (a, b) => DifficultyRank(a.difficulty).CompareTo(DifficultyRank(b.difficulty)));
        }
        else
        {
            return; // Μέτρια απόδοση -- καμία αλλαγή στη σειρά.
        }

        System.Array.Copy(remaining, 0, sessionRounds, currentRoundIndex, remainingCount);

        gameLogger?.Log(
            $"ADAPTIVE DDA | SuccessRate={successRate:F2} | Reordered {remainingCount} remaining rounds " +
            (successRate >= 0.70f ? "(harder first)" : "(easier first)")
        );
    }

    private static int DifficultyRank(DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case DifficultyLevel.Easy: return 0;
            case DifficultyLevel.Medium: return 1;
            case DifficultyLevel.Hard: return 2;
            default: return 1;
        }
    }
}