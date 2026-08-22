using UnityEngine;
using UnityEngine.SceneManagement;
public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "MainMenuNew";

    private void Start()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToMainMenu();
        }
        else
        {
            Debug.LogWarning("BootstrapLoader: GameFlowManager.Instance was null, falling back to direct LoadScene.");
            SceneManager.LoadScene(firstSceneName);
        }
    }
}