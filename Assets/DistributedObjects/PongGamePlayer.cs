using P2PPlugin.Network;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PongGamePlayer : P2PNetworkObject
{
    public static PongGamePlayer localPongGame = null;
    public static PongGamePlayer remotePongGame = null;
    public static GameObject remotePongPlayerGO = null;

    [P2PDistributed]
    public int pongGameUID;

    [P2PToPeer]
    public long toPeer;

    [P2PDistributed]
    public int playerNumber;   // 0 - left/primary, 1 - right/secondary

    [P2PSkip]
    public int[] _playerScores = new int[2] { 0, 0 };
    [P2PDistributed]
    public int[] PlayerScores
    {
        set
        {
            _playerScores = value;
            if (!isLocal)
            {
                P2PPongGameManager.Instance.UpdateGameoverText(this);
            }
        }
        get
        {
            return _playerScores;
        }
    }

    public int _winningplayer = -1;
    [P2PDistributed]
    public int WinningPlayer
    {
        set
        {
            _winningplayer = value;
            if (!isLocal)
            {
                if (value != -1)
                {
                    P2PPongGameManager.Instance.EndGame(value);
                } else
                { // restarting
                    P2PPongGameManager.Instance.gameoverPanel.SetActive(false);
                }
            }
        }
        get
        {
            return _winningplayer;
        }
    }

    [P2PSkip]
    public float _position;
    [P2PDistributed]
    public float position
    {
        set
        {
            _position = value;
            if (remotePongGame != null && remotePongPlayerGO != null)
            {
                SetupPlayerGameObject(remotePongGame, remotePongPlayerGO);
            }
        }
        get
        {
            return _position;
        }
    }
    [P2PSkip]
    public Vector3 _ballPosition = new Vector3(0.5f, 0.5f, 0);
    [P2PDistributed]
    public Vector3 BallPosition
    {
        set
        {
            _ballPosition = value;
            if (!isLocal)
            {
                P2PPongGameManager.Instance.ballGameObject.transform.SetLocalPositionAndRotation(PongResizeListener.Instance.normalToWorldPosition(value), Quaternion.identity);
            }
        }
        get
        {
            return _ballPosition;
        }
    }

    [P2PSkip]
    public static long RemotePeer
    {
        get
        {
            if (localPongGame != null)
            {
                if (localPongGame.isLocal)
                {
                    return localPongGame.toPeer;
                }
                return localPongGame.sourceComputerID;
            }
            return 0L;
        }
    }
    static public void SetupPlayerGameObject(PongGamePlayer pgp, GameObject go)
    {
        SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
        Vector3 oldPosition = go.transform.position;
        if (pgp.playerNumber == 0)
        {
            go.transform.SetPositionAndRotation(new Vector3(oldPosition.x, pgp.position, oldPosition.z), Quaternion.identity);
            spriteRenderer.color = Color.blue;
        } else
        {
            go.transform.SetPositionAndRotation(new Vector3(oldPosition.x, pgp.position, oldPosition.z), Quaternion.identity);
            spriteRenderer.color = Color.red;
        }
    }
    public void AfterInsertRemote()
    {
        if (localPongGame == null)
        {
            remotePongGame = this;
            localPongGame = new PongGamePlayer();
            localPongGame.pongGameUID = pongGameUID;
            localPongGame.toPeer = sourceComputerID;
            localPongGame.playerNumber = 1;  // right/secondary
            localPongGame.Insert();
        }
        if (PlayerControl.Instance.prefabForRemotePlayer != null && remotePongPlayerGO == null)
        {
            remotePongPlayerGO = UnityEngine.Object.Instantiate(PlayerControl.Instance.prefabForRemotePlayer);

            remotePongPlayerGO.name = "Remote Player";
            remotePongPlayerGO.transform.SetParent(PlayerControl.Instance.playerParent.transform);
            remotePongGame = this;
            SetupPlayerGameObject(this, remotePongPlayerGO);
            PlayerControl.Instance.setPlayerGO(playerNumber, remotePongPlayerGO);
        }
    }
    public void AfterDeleteRemote()
    {
        if (remotePongPlayerGO != null)
        {
            Object.Destroy(remotePongPlayerGO);
            remotePongPlayerGO = null;    
        }
        BackToLobby.Instance.BackToLobbyAction();
    }
    public PongGamePlayer()
    {
    }

    public void AddScore(int playerId, int score)
    {
        PlayerScores[playerId] += score;
    }
    public string getDisplayScores()
    {
        if (PlayerScores == null)
        {
            return "0 : 0";
        }
        if (playerNumber == 0)
        {
            return PlayerScores[0] + " : " + PlayerScores[1];
        } else
        {
            return PlayerScores[1] + " : " + PlayerScores[0];
        }
    }
    static public void CreateLocalPongGamePlayer(long toPeerID)
    {
        localPongGame = new PongGamePlayer();
        localPongGame.toPeer = toPeerID;
        localPongGame.playerNumber = 0;
        localPongGame.pongGameUID = (int)P2PObject.IntRandFunc();
        localPongGame.Insert();
    }
}
