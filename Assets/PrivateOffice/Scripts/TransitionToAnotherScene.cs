using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionToAnotherScene : MonoBehaviour
{
    public void Transition()
    {
        SceneManager.LoadScene("Scene_2");
    }
}
