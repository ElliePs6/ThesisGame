using TMPro;
using UnityEngine;

public class AuthorityToastUI : MonoBehaviour
{
    [Header("Toast Root")]
    [SerializeField] private GameObject toastPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text authorityMessageText;
    [SerializeField] private TMP_Text influenceChangeText;

    [Header("Outcome Icons")]
    [SerializeField] private GameObject goodChoiceCircle;
    [SerializeField] private GameObject acceptableChoiceCircle;
    [SerializeField] private GameObject badChoiceCircle;
    [SerializeField] private GameObject veryBadChoiceCircle;

    private void Awake()
    {
        Hide();
    }

    public void Show(
        ActionOutcomeQuality quality,
        string message,
        int influenceChange)
    {
        HideAllIcons();

        if (authorityMessageText != null)
        {
            authorityMessageText.text = message;
        }

        if (influenceChangeText != null)
        {
            string formattedChange = influenceChange >= 0
                ? $"+{influenceChange}"
                : influenceChange.ToString();

            influenceChangeText.text = $"Influence {formattedChange}";
        }

        ShowIconForQuality(quality);

        if (toastPanel != null)
        {
            toastPanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        HideAllIcons();

        if (toastPanel != null)
        {
            toastPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void ShowIconForQuality(ActionOutcomeQuality quality)
    {
        switch (quality)
        {
            case ActionOutcomeQuality.Best:
                SetActiveSafe(goodChoiceCircle, true);
                break;

            case ActionOutcomeQuality.Acceptable:
                SetActiveSafe(acceptableChoiceCircle, true);
                break;

            case ActionOutcomeQuality.Bad:
                SetActiveSafe(badChoiceCircle, true);
                break;

            case ActionOutcomeQuality.VeryBad:
                SetActiveSafe(veryBadChoiceCircle, true);
                break;

            default:
                Debug.LogWarning(
                    $"AuthorityToastUI: No icon mapping for quality '{quality}'.",
                    this
                );
                break;
        }
    }

    private void HideAllIcons()
    {
        SetActiveSafe(goodChoiceCircle, false);
        SetActiveSafe(acceptableChoiceCircle, false);
        SetActiveSafe(badChoiceCircle, false);
        SetActiveSafe(veryBadChoiceCircle, false);
    }

    private static void SetActiveSafe(GameObject target, bool isActive)
    {
        if (target != null)
        {
            target.SetActive(isActive);
        }
    }
}