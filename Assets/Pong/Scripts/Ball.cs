// using Unity.Netcode;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public bool pause = false;
    private Vector2 direction = Vector2.right;
    public float speed = 10f;
    private readonly float randomRefectionIntensity = 0.1f;
    
    private void FixedUpdate()
    {
        if (pause || PongGamePlayer.localPongGame == null || PongGamePlayer.localPongGame.playerNumber == 1 || !P2PPongGameManager.Instance.isPlaying)
        {
            return;
        }
        var distance = speed * Time.deltaTime;
        var hit = Physics2D.Raycast(transform.position, direction, distance);

        if (hit.collider != null)
        {
            direction = Vector2.Reflect(direction, hit.normal);
            direction += Random.insideUnitCircle * randomRefectionIntensity;

            var goalpost = hit.collider.GetComponent<Goalpost>();
            if (goalpost != null)
            {
                P2PPongGameManager.Instance.AddScore(goalpost.PlayerId, 1);
            }
        }

        transform.position = (Vector2)transform.position + direction * distance;
        PongGamePlayer.localPongGame.BallPosition = PongResizeListener.Instance.worldToNormalPosition(transform.position);
        PongGamePlayer.localPongGame.UpdateAllFields();
    }
}