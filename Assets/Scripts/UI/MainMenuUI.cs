using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Input")]
    public TMP_InputField usernameInput;

    public void OnNewGameClicked()
    {
        string username = usernameInput != null
            ? usernameInput.text
            : "Anonymous";

        GameFlowManager.Instance.SetParticipant(username);
        GameFlowManager.Instance.StartFirstBlock();
    }

    public void OnResultsClicked()
    {
        GameFlowManager.Instance.GoToResults();
    }

    public void OnExitClicked()
    {
        GameFlowManager.Instance.ExitGame();
    }
}