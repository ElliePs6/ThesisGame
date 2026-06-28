using System;
using UnityEngine;

public enum ActionOutcomeQuality
{
    Best,
    Acceptable,
    Bad,
    VeryBad
}

[Serializable]
public class ActionOutcome
{
    public PlayerActionType action;

    public ActionOutcomeQuality quality;

    [TextArea(2, 4)]
    public string npcResponse;

    public string nextNpcState;

    public int influenceChange;
}