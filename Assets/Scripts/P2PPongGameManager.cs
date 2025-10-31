using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class P2PPongGameManager : MonoBehaviour
{
    public static P2PPongGameManager Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<P2PPongGameManager>();

            return instance;
        }
    }
    private const int WinScore = 10;
    private static P2PPongGameManager instance;

    public Text scoreText;
    public GameObject ballPrefab;

    public Text gameoverText;
    public GameObject gameoverPanel;

    public GameObject restartGameButton;

    public Goalpost[] goalposts;

    [HideInInspector]
    public GameObject ballGameObject = null;

    [HideInInspector]
    public bool isPlaying = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void AddScore(int playerId, int score)
    {
        PongGamePlayer.localPongGame.AddScore(playerId, score);
        UpdateGameoverText(PongGamePlayer.localPongGame);

        if (PongGamePlayer.localPongGame.PlayerScores[playerId] >= WinScore)
        {
            PongGamePlayer.localPongGame.WinningPlayer = playerId;
            EndGame(playerId);
        }
    }
    public void EndGame(int playerId)
    {
        if (PongGamePlayer.localPongGame.playerNumber == playerId)
        {
            gameoverText.text = "You Win!";
        }
        else
        {
            gameoverText.text = "You Lose!";
        }
        restartGameButton.SetActive(PongGamePlayer.localPongGame.playerNumber == 0);
        gameoverPanel.SetActive(true);
        isPlaying = false;
    }
    public void UpdateGameoverText(PongGamePlayer pongGame)
    {
        scoreText.text = pongGame.getDisplayScores();
    }
    void Start() {
        gameoverPanel.SetActive(false);
        if (ballGameObject == null)
        {
            ballGameObject = Instantiate(ballPrefab, Vector2.zero, Quaternion.identity);
        }
        if (goalposts != null && goalposts.Length == 2)
        {
            goalposts[0].PlayerId = 1;
            goalposts[1].PlayerId = 0;
        } else
        {
            Debug.Log("WARNING: Need 2 goldposts but have " + goalposts.Length);
        }
        isPlaying = true;
    }
    void OnDestroy()
    {
        if (ballGameObject != null)
        {
            Destroy(ballGameObject);
            ballGameObject = null;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /**/
    }

    public void RestartGame()
    {
        PongGamePlayer.localPongGame.PlayerScores[0] = 0;
        PongGamePlayer.localPongGame.PlayerScores[1] = 0;
        PongGamePlayer.localPongGame.WinningPlayer = -1;
        PongGamePlayer.localPongGame.UpdateAllFields();

        UpdateGameoverText(PongGamePlayer.localPongGame);
        isPlaying = true;
        ballGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        restartGameButton.SetActive(false);
        gameoverPanel.SetActive(false);
    }
    void Update()
    {
        
    }
}
