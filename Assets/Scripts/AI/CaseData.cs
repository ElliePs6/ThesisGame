using System;
using UnityEngine;

[Serializable]
public class CaseData
{
    public string caseId;

    [TextArea(2, 4)]
    public string situation;

    public string npcState;

    public string[] contextTags;

    public PlayerActionType recommendedAction;

    [TextArea(2, 4)]
    public string rationale;

    [TextArea(2, 4)]
    public string alternatives;

    [TextArea(2, 4)]
    public string expectedOutcome;

    [Range(0, 1)]
    public float expertConfidence = 0.75f;

    [Header("Player-Facing Dialogue (Adaptive mode)")]
    [Tooltip("Φυσική ατάκα NPC που εμφανίζεται στον παίκτη ΜΟΝΟ όταν αυτό το case ταιριάζει με αρκετά υψηλό score στο Adaptive mode. Αν είναι κενό, γίνεται fallback στο RoundData.outcomes[].npcResponse (ίδιο με το Static mode).")]
    [TextArea(2, 4)]
    public string adaptiveNpcLine;
}