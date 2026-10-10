using UnityEngine;

public class NpcAnimationController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator npcAnimator;

    private static readonly int AvoidantTrigger =
        Animator.StringToHash("Avoidant");

    private static readonly int DefensiveTrigger =
        Animator.StringToHash("Defensive");

    private static readonly int UncertainTrigger =
        Animator.StringToHash("Uncertain");

    private static readonly int EscalatedTrigger =
        Animator.StringToHash("Escalated");

    private static readonly int CooperativeTrigger =
        Animator.StringToHash("Cooperative");

    private void Awake()
    {
        if (npcAnimator == null)
        {
            npcAnimator = GetComponent<Animator>();
        }

        if (npcAnimator == null)
        {
            Debug.LogError(
                "NpcAnimationController: No Animator was assigned or found.",
                this
            );
        }
    }

    public void PlayState(string npcState)
    {
        if (npcAnimator == null)
            return;

        ResetReactionTriggers();

        string normalizedState = NormalizeState(npcState);

        switch (normalizedState)
        {
            case "avoidance":
                npcAnimator.SetTrigger(AvoidantTrigger);
                break;

            case "defensive":
            case "resistant":
                npcAnimator.SetTrigger(DefensiveTrigger);
                break;

            case "inconsistent":
            case "uncertain":
            case "reflective":
            case "confident-but-uncertain":
            case "cooperative-but-incomplete":
            case "stagnant":
                npcAnimator.SetTrigger(UncertainTrigger);
                break;

            case "escalated":
            case "angry":
            case "impatient":
                npcAnimator.SetTrigger(EscalatedTrigger);
                break;

            case "cooperative":
            case "ready":
            case "resolved":
            case "sharing-insight":
                npcAnimator.SetTrigger(CooperativeTrigger);
                break;

            case "firm":
            case "neutral":
            case "":
                // Το Neutral είναι το default state.
                // Δεν χρειάζεται trigger.
                break;

            default:
                Debug.LogWarning(
                    $"NpcAnimationController: No animation mapping found for state '{npcState}'."
                );
                break;
        }
    }

    private void ResetReactionTriggers()
    {
        npcAnimator.ResetTrigger(AvoidantTrigger);
        npcAnimator.ResetTrigger(DefensiveTrigger);
        npcAnimator.ResetTrigger(UncertainTrigger);
        npcAnimator.ResetTrigger(EscalatedTrigger);
        npcAnimator.ResetTrigger(CooperativeTrigger);
    }

    private string NormalizeState(string npcState)
    {
        if (string.IsNullOrWhiteSpace(npcState))
            return "";

        return npcState.Trim().ToLowerInvariant();
    }
}