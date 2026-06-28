using UnityEngine;

public class ModeSelectionUI : MonoBehaviour
{
    public void OnStaticModeClicked()
    {
        GameFlowManager.Instance.StartSession(AIMode.Static);
    }

    public void OnAdaptiveModeClicked()
    {
        GameFlowManager.Instance.StartSession(AIMode.Adaptive);
    }

    public void OnBackClicked()
    {
        GameFlowManager.Instance.GoToMainMenu();
    }
}