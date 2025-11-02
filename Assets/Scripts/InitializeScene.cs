using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeLobbyScene : MonoBehaviour
{
    // public Scene sceneToLoad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // SceneManager.LoadScene(sceneToLoad.name);
        SceneManager.LoadScene("Lobby");
    }
}
