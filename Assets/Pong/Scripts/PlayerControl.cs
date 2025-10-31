using P2PPlugin.Network;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public static PlayerControl Instance = null;
    public PlayerControl()
    {
        Instance = this;
    }
    private float moveInput;
    private PlayerInputsForPong controls;
    public float speed = 5f;

    public GameObject prefabForRemotePlayer;

    public PongResizeListener pongResizeListener;

    public GameObject playerParent;

    public void setPlayerGO(int playerNumber, GameObject go)
    {
        if (playerNumber == 0)
        {
            pongResizeListener.leftPlayer = go;
        }
        else
        {
            pongResizeListener.rightPlayer = go;
        }
        pongResizeListener.fireAdjustUIToSize();
    }
    private void Awake()
    {
        controls = new PlayerInputsForPong();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<float>();
        controls.Player.Move.canceled += _ => moveInput = 0;
    }

    void OnEnable() {
        controls.Enable();                 // enable ALL maps
        controls.Player.Enable();          // belt & suspenders: enable the Player map too
    }
    void OnDisable()
    {
        controls.Player.Disable();
        controls.Disable();
    }
    bool isPlayerGOSetup = false;
    private void Update()
    {
        if (PongGamePlayer.localPongGame == null)
        {
            return;
        }

        if (!isPlayerGOSetup)
        {
            PongGamePlayer.SetupPlayerGameObject(PongGamePlayer.localPongGame, gameObject);
            setPlayerGO(PongGamePlayer.localPongGame.playerNumber, gameObject);
            isPlayerGOSetup = true;
        }

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            var distance = moveInput * speed * Time.deltaTime;
            var position = gameObject.transform.position;
            position.y = gameObject.transform.position.y + distance;
            position.y = Mathf.Clamp(position.y, -4.5f, 4.5f);
            gameObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            PongGamePlayer.localPongGame.position = position.y;
            PongGamePlayer.localPongGame.UpdateAllFields();
        }        
    }
}
