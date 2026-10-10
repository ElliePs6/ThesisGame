public static class DefaultRoundLibrary
{
    public static RoundData[] GetDefaultRoundPool()
    {
        RoundData[] roundPool;

        roundPool = new RoundData[14];

        roundPool[0] = new RoundData
        {
            scenarioId = "ROUND-001",
            statement = "Ο πελάτης αποφεύγει να δώσει σαφείς λεπτομέρειες και κρατάει την απάντησή του σύντομη.",
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
                    npcResponse = "Εντάξει... μπορώ να σου πω κάτι, αλλά ακόμα δεν νιώθω άνετα με όλο αυτό.",
                    nextNpcState = "defensive",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Νιώθω πιο άνετα τώρα, αλλά υπάρχει κάτι που δεν σου έχω πει ακόμα.",
                    nextNpcState = "avoidance",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Έτσι όπως μου μιλάς νιώθω πίεση, και δεν μου αρέσει καθόλου.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Πάμε πολύ γρήγορα για μένα, δεν νομίζω ότι μπορώ να συνεχίσω έτσι.",
                    nextNpcState = "escalated",
                    influenceChange = -3
                }
            }
        };

        roundPool[1] = new RoundData
        {
            scenarioId = "ROUND-002",
            statement = "Ο πελάτης γίνεται έντονος και αμυντικός κατά τη διάρκεια της συζήτησης.",
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
                    npcResponse = "Εντάξει, ηρεμώ λίγο... μπορώ να συνεχίσω τώρα.",
                    nextNpcState = "avoidance",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Μπορώ να σου πω κάτι, αλλά ακόμα νιώθω ότι πρέπει να προστατευτώ.",
                    nextNpcState = "defensive",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Αυτό μου φαίνεται σαν πίεση, και με κάνει να κλείνομαι ακόμα πιο πολύ.",
                    nextNpcState = "escalated",
                    influenceChange = -2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Δεν έχω ηρεμήσει ακόμα, και το να προχωράμε τώρα με κάνει να αντιστέκομαι περισσότερο.",
                    nextNpcState = "escalated",
                    influenceChange = -3
                }
            }
        };

        roundPool[2] = new RoundData
        {
            scenarioId = "ROUND-003",
            statement = "Ο πελάτης δίνει πληροφορίες που έρχονται σε αντίθεση με προηγούμενη δήλωσή του.",
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
                    npcResponse = "Έχεις δίκιο, δεν ταιριάζει με ό,τι είπα πριν. Άσε με να το ξεκαθαρίσω.",
                    nextNpcState = "defensive",
                    influenceChange = 5
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Μπορώ να σου δώσω διευκρινήσεις για την τωρινή δήλωση, χωρίς να αναφερθώ στις προηγούμενες δηλώσεις.",
                    nextNpcState = "inconsistent",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Θέλω να σε βοηθήσω, αλλά αυτό που δεν κολλάει με ό,τι είπα πριν... δεν το έχω εξηγήσει ακόμα.",
                    nextNpcState = "inconsistent",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Δεν έχουμε ξεκαθαρίσει αυτή την αντίφαση ακόμα, και αυτό με κάνει να αμφιβάλλω για την απόφαση.",
                    nextNpcState = "defensive",
                    influenceChange = -3
                }
            }
        };

        roundPool[3] = new RoundData
        {
            scenarioId = "ROUND-004",
            statement = "Ο πελάτης φαίνεται να έχει κατακλυστεί και η συζήτηση γίνεται ασταθής.",
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
                    npcResponse = "Εντάξει... νιώθω λίγο πιο σταθερός τώρα. Μπορούμε να συνεχίσουμε.",
                    nextNpcState = "defensive",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Άλλες ερωτήσεις τώρα με πιέζουν ακόμα πιο πολύ, δεν ξέρω αν μπορώ να σου πω κάτι χρήσιμο.",
                    nextNpcState = "escalated",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Αυτό με αναστατώνει ακόμα πιο πολύ, δεν μπορώ να συνεχίσω έτσι.",
                    nextNpcState = "escalated",
                    influenceChange = -3
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Δεν έχω σταθεροποιηθεί ακόμα, και το να προχωράμε τώρα δεν μου φαίνεται καλή ιδέα.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                }
            }
        };

        roundPool[4] = new RoundData
        {
            scenarioId = "ROUND-005",
            statement = "Ο πελάτης δίνει μια ασαφή απάντηση που φαίνεται να κρύβει μια σημαντική λεπτομέρεια.",
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
                    npcResponse = "Εντάξει... θα σου πω τι ακριβώς συμβαίνει, για να το ξεκαθαρίσουμε.",
                    nextNpcState = "defensive",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Θέλω να σε βοηθήσω, αλλά υπάρχει κάτι που δεν σου έχω πει ακόμα.",
                    nextNpcState = "avoidance",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Έτσι όπως το λες, κλείνομαι ακόμα πιο πολύ, και δεν έχουμε καν ξεκαθαρίσει τι λείπει.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Λείπει ακόμα κάτι σημαντικό, δεν νομίζω ότι πρέπει να προχωρήσουμε τώρα.",
                    nextNpcState = "escalated",
                    influenceChange = -3
                }
            }
        };

        roundPool[5] = new RoundData
        {
            scenarioId = "ROUND-006",
            statement = "Ο πελάτης φτάνει σε ένα ξεκάθαρο σημείο απόφασης και η συζήτηση χρειάζεται προώθηση.",
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
                    npcResponse = "Ναι, νομίζω ότι είναι η στιγμή να προχωρήσουμε.",
                    nextNpcState = "resolved",
                    influenceChange = 6
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Νομίζω το έχουμε ήδη ξεκαθαρίσει, άλλες ερωτήσεις τώρα με καθυστερούν μόνο.",
                    nextNpcState = "firm",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Είμαι εντάξει, αλλά χρειάζομαι να προχωρήσουμε σε κάτι συγκεκριμένο.",
                    nextNpcState = "firm",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Δεν χρειάζεται αυτή η ένταση τώρα, πρέπει απλά να αποφασίσουμε.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                }
            }
        };

        roundPool[6] = new RoundData
        {
            scenarioId = "ROUND-007",
            statement = "Ο πελάτης δίνει μια συνεργάσιμη απάντηση, αλλά μια σημαντική λεπτομέρεια φαίνεται να λείπει ακόμα.",
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
                    npcResponse = "Έχεις δίκιο, υπάρχει κάτι που δεν σου είχα πει. Άσε με να στο εξηγήσω καλύτερα.",
                    nextNpcState = "cooperative",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Ξέρω ότι δεν το εξηγώ πολύ καθαρά, αλλά χαίρομαι που με καταλαβαίνεις. Υπάρχει όμως ακόμα κάτι που δεν έχουμε δει καλά.",
                    nextNpcState = "cooperative-but-incomplete",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Με μπερδεύει αυτή η ξαφνική πίεση, δυσκολεύομαι να σου απαντήσω σωστά τώρα.",
                    nextNpcState = "defensive",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Μπορούμε να προχωρήσουμε, νομίζω... αλλά έχω κάτι στο μυαλό μου που ίσως είναι σημαντικό, απλά δεν είμαι σίγουρος ακόμα.",
                    nextNpcState = "uncertain",
                    influenceChange = 1
                }
            }
        };

        roundPool[7] = new RoundData
        {
            scenarioId = "ROUND-008",
            statement = "Ο πελάτης επαναλαμβάνει έναν ισχυρισμό που ακούγεται σίγουρος, αλλά οι διαθέσιμες πληροφορίες παραμένουν αβέβαιες.",
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
                    npcResponse = "Έχεις δίκιο, πάμε πιο αργά. Μπορώ να σου εξηγήσω περισσότερα πριν αποφασίσουμε.",
                    nextNpcState = "reflective",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Είμαι ήρεμος, αλλά ξέρω πως δεν έχω εξηγήσει καλά πόσο σίγουρος είμαι πραγματικά.",
                    nextNpcState = "confident-but-uncertain",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Νιώθω ότι πρέπει να υποστηρίξω αυτό που είπα. Μπορώ να σου πω περισσότερα, αλλά με αγχώνει λίγο.",
                    nextNpcState = "defensive",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Δεσμευόμαστε πολύ νωρίς, δεν είμαστε καν σίγουροι για τα βασικά. Δεν μου φαίνεται σωστό.",
                    nextNpcState = "escalated",
                    influenceChange = -4
                }
            }
        };

        roundPool[8] = new RoundData
        {
            scenarioId = "ROUND-009",
            statement = "Ο πελάτης αντιδρά έντονα αφού νιώθει παρεξηγημένος και η συζήτηση αρχίζει να κλιμακώνεται.",
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
                    npcResponse = "Νιώθω ότι με καταλαβαίνεις επιτέλους. Αυτό με βοηθά να ηρεμήσω.",
                    nextNpcState = "defensive",
                    influenceChange = 5
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Αυτή η ερώτηση τώρα μου φαίνεται σαν πίεση, δεν έχω όρεξη να συνεχίσω έτσι.",
                    nextNpcState = "escalated",
                    influenceChange = -2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Αναστατώνομαι ακόμα πιο πολύ τώρα, δεν μπορώ να συνεχίσω έτσι.",
                    nextNpcState = "escalated",
                    influenceChange = -4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Προσπαθείς να με κάνεις να προχωρήσω, αλλά ακόμα είμαι πολύ φορτισμένος για αυτό.",
                    nextNpcState = "resistant",
                    influenceChange = -3
                }
            }
        };

        roundPool[9] = new RoundData
        {
            scenarioId = "ROUND-010",
            statement = "Ο πελάτης έχει διευκρινίσει το βασικό θέμα και είναι έτοιμος για ένα συγκεκριμένο επόμενο βήμα.",
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
                    npcResponse = "Ναι, νομίζω ήρθε η στιγμή. Νιώθω ότι φτάνουμε κάπου καλό.",
                    nextNpcState = "resolved",
                    influenceChange = 6
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Είμαι εντάξει, αλλά χρειάζομαι να μου πεις τι ακολουθεί.",
                    nextNpcState = "ready",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Νομίζω το έχουμε ήδη ξεκαθαρίσει, δεν χρειάζονται άλλες ερωτήσεις.",
                    nextNpcState = "impatient",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Δεν χρειάζεται αυτή η τριβή τώρα, θα μπορούσαμε ήδη να έχουμε καταλήξει.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                }
            }
        };

        roundPool[10] = new RoundData
        {
            scenarioId = "ROUND-011",
            statement = "Ο πελάτης έχει μιλήσει αρκετές φορές για το ίδιο θέμα χωρίς να προκύψει κάτι νέο, και δεν βλέπει πια λόγο να το ξανασκεφτεί — δεν το θεωρεί πρόβλημα, απλά έτσι είναι τα πράγματα.",
            caseType = CaseType.Contradiction,
            difficulty = DifficultyLevel.Easy,
            commitTiming = CommitTiming.Middle,
            roundPosition = RoundPosition.Development,
            npcState = "stagnant",
            contextTags = new[] { "plateaued-progress", "comfortable-pattern", "needs-insight" },
            preferredAction = PlayerActionType.Challenge,
            outcomes = new[]
          {
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "Με αιφνιδίασες λίγο... αλλά ίσως έχεις δίκιο. Μάλλον έχω μείνει στα ίδια χωρίς να δοκιμάσω κάτι διαφορετικό.",
                    nextNpcState = "reflective",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Μπορώ να σου πω περισσότερα για το πώς τα πάω, αλλά ειλικρινά νιώθω ότι όλα είναι όπως πριν και δεν με ενοχλεί.",
                    nextNpcState = "uncertain",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Ευχαριστώ που με στηρίζεις, με βοηθάει να νιώθω καλά όπως είμαι. Δεν βλέπω πραγματικά λόγο να αλλάξω κάτι.",
                    nextNpcState = "firm",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Να κλείσουμε κάτι τώρα; Δεν έχουμε δει τι θέλω πραγματικά να αλλάξει, οπότε δεν καταλαβαίνω τι θα κλείσουμε.",
                    nextNpcState = "defensive",
                    influenceChange = -3
                }
            }
        };
        roundPool[11] = new RoundData
        {
            scenarioId = "ROUND-012",
            statement = "Ο πελάτης αρνείται συστηματικά να δώσει οποιαδήποτε συγκεκριμένη λεπτομέρεια, ακόμα και όταν πιέζεται ελαφρά.",
            caseType = CaseType.InformationGap,
            difficulty = DifficultyLevel.Hard,
            commitTiming = CommitTiming.Middle,
            roundPosition = RoundPosition.Development,
            npcState = "avoidance",
            contextTags = new[] { "unclear-details", "information-gap", "high-pressure" },
            preferredAction = PlayerActionType.Probe,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "Εντάξει... μετά από όλες αυτές τις ερωτήσεις, μπορώ επιτέλους να σου πω κάτι χρήσιμο.",
                    nextNpcState = "defensive",
                    influenceChange = 5
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Χαλαρώνω λίγο, αλλά δεν είμαι έτοιμος να μπω σε λεπτομέρειες ακόμα.",
                    nextNpcState = "avoidance",
                    influenceChange = 1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Αυτό μου φαίνεται σαν πίεση, και με κάνει να κλείνομαι ακόμα πιο πολύ.",
                    nextNpcState = "escalated",
                    influenceChange = -3
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Δεν έχουμε καν τα βασικά ακόμα, δεν μπορώ να προχωρήσω έτσι.",
                    nextNpcState = "escalated",
                    influenceChange = -4
                }
            }
        };

        roundPool[12] = new RoundData
        {
            scenarioId = "ROUND-013",
            statement = "Ο πελάτης δίνει μια γενική απάντηση χωρίς πολλές λεπτομέρειες, αλλά δεν δείχνει ιδιαίτερη αντίσταση.",
            caseType = CaseType.InformationGap,
            difficulty = DifficultyLevel.Easy,
            commitTiming = CommitTiming.Early,
            roundPosition = RoundPosition.Development,
            npcState = "avoidance",
            contextTags = new[] { "unclear-details", "information-gap", "low-pressure" },
            preferredAction = PlayerActionType.Probe,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "Φυσικά, μπορώ να σου πω κάτι πιο συγκεκριμένο.",
                    nextNpcState = "cooperative",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Είμαι εντάξει, αλλά ξέρω πως αυτό που είπα ήταν αρκετά γενικό.",
                    nextNpcState = "avoidance",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Δεν χρειάζεται αυτή η πίεση για κάτι τόσο απλό, με ενοχλεί λίγο.",
                    nextNpcState = "defensive",
                    influenceChange = -1
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Προχωράμε χωρίς να έχεις ζητήσει καμία διευκρίνιση από μένα.",
                    nextNpcState = "avoidance",
                    influenceChange = -2
                }
            }
        };

        roundPool[13] = new RoundData
        {
            scenarioId = "ROUND-014",
            statement = "Ο πελάτης αντιδρά σε μια πρόταση δράσης, λέγοντας ότι αυτό που τον βοήθησε ήταν η στήριξη ανθρώπων γύρω του, όχι κάποιο σχέδιο.",
            caseType = CaseType.Defensive,
            difficulty = DifficultyLevel.Easy,
            commitTiming = CommitTiming.Middle,
            roundPosition = RoundPosition.Development,
            npcState = "sharing-insight",
            contextTags = new[] { "positive-disclosure", "peer-support", "autonomy" },
            preferredAction = PlayerActionType.Support,
            outcomes = new[]
            {
                new ActionOutcome
                {
                    action = PlayerActionType.Support,
                    quality = ActionOutcomeQuality.Best,
                    npcResponse = "Νιώθω ότι με ακούς πραγματικά. Η στήριξη των άλλων ήταν αυτό που με βοήθησε, και χαίρομαι που το παίρνεις υπόψη.",
                    nextNpcState = "cooperative",
                    influenceChange = 4
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Probe,
                    quality = ActionOutcomeQuality.Acceptable,
                    npcResponse = "Μπορώ να σου πω περισσότερα για το πώς με βοήθησαν, αλλά δεν ξέρω αν αυτό θα μετρήσει στο τελικό σχέδιο.",
                    nextNpcState = "uncertain",
                    influenceChange = 2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Challenge,
                    quality = ActionOutcomeQuality.Bad,
                    npcResponse = "Νιώθω ότι αμφισβητείς κάτι που πραγματικά με βοήθησε. Δεν καταλαβαίνω γιατί πρέπει να το αγνοήσω.",
                    nextNpcState = "defensive",
                    influenceChange = -2
                },
                new ActionOutcome
                {
                    action = PlayerActionType.Commit,
                    quality = ActionOutcomeQuality.VeryBad,
                    npcResponse = "Μου παρουσιάζεις ήδη ένα έτοιμο πλάνο, χωρίς να έχουμε μιλήσει γι' αυτό που με βοήθησε. Νιώθω ότι η γνώμη μου δεν μετράει.",
                    nextNpcState = "avoidance",
                    influenceChange = -3
                }
            }
        };

        // Counterbalanced within-subjects design: κάθε round ανήκει σε ένα
        // από τα δύο ισοδύναμα υποσύνολα (Α/Β) -- κάθε participant παίζει
        // κάθε AI mode με διαφορετικό υποσύνολο, ώστε να μην ξαναδεί τις
        // ίδιες σωστές απαντήσεις στο δεύτερο block.
        // Subset A (2 Easy / 3 Medium / 2 Hard): ROUND-001, 002, 004, 007, 010, 011, 012
        // Subset B (2 Easy / 3 Medium / 2 Hard): ROUND-003, 005, 006, 008, 009, 013, 014
        roundPool[0].subsetGroup = RoundSubsetGroup.A;
        roundPool[1].subsetGroup = RoundSubsetGroup.A;
        roundPool[2].subsetGroup = RoundSubsetGroup.B;
        roundPool[3].subsetGroup = RoundSubsetGroup.A;
        roundPool[4].subsetGroup = RoundSubsetGroup.B;
        roundPool[5].subsetGroup = RoundSubsetGroup.B;
        roundPool[6].subsetGroup = RoundSubsetGroup.A;
        roundPool[7].subsetGroup = RoundSubsetGroup.B;
        roundPool[8].subsetGroup = RoundSubsetGroup.B;
        roundPool[9].subsetGroup = RoundSubsetGroup.A;
        roundPool[10].subsetGroup = RoundSubsetGroup.A;
        roundPool[11].subsetGroup = RoundSubsetGroup.A;
        roundPool[12].subsetGroup = RoundSubsetGroup.B;
        roundPool[13].subsetGroup = RoundSubsetGroup.B;

        return roundPool;
    }
}