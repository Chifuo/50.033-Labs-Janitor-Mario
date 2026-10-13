using System;
using UnityEngine;

public class GoombaShoo : MonoBehaviour
{
    // Space shoos a Goomba while Mario is touching it.
    [SerializeField, Min(0f)] private float retreatSpeed = 5f;
    [SerializeField, Min(0f)] private float retreatDuration = 0.3f;
    private PlayerMovement fightingPlayer;
    private Vector2 retreatDirection;
    private float retreatUntil;
    private EnemyMovement movement;
    private GoombaTrashCarrier trashCarrier;
    public event Action Shooed;

    public bool IsInteracting => fightingPlayer != null;
    public bool IsRetreating => Time.time < retreatUntil;

    public void Initialize(EnemyMovement owner, GoombaTrashCarrier carrier)
    {
        movement = owner;
        trashCarrier = carrier;
    }

    public void CheckContact()
    {
        if (fightingPlayer != null)
        {
            if (!fightingPlayer.isActiveAndEnabled)
            {
                EndFight();
                // return; // Old stop-on-contact behaviour.
            }
            // return; // Old stop-on-contact behaviour.
        }
    }

    public void TryShoo(PlayerMovement player)
    {
        if (!isActiveAndEnabled || player == null || !player.isActiveAndEnabled ||
            fightingPlayer != player || movement == null || !movement.isActiveAndEnabled ||
            movement.IsGameOver || IsRetreating)
            return;

        ShooAway();
    }

    public Vector2 GetRetreatVelocity()
    {
        Vector2 step = retreatDirection * Mathf.Max(0f, retreatSpeed) * Time.fixedDeltaTime;
        return movement.CanTravel(movement.Position, movement.Position + step)
            ? step / Time.fixedDeltaTime : Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        InteractWithPlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        InteractWithPlayer(collision.collider);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (fightingPlayer != null &&
            collision.collider.GetComponentInParent<PlayerMovement>() == fightingPlayer)
            EndFight();
    }

    private void InteractWithPlayer(Collider2D other)
    {
        if (movement == null || !movement.isActiveAndEnabled || fightingPlayer != null || Time.time < retreatUntil || !other.CompareTag("Player") ||
            movement.IsGameOver)
            return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null || !player.isActiveAndEnabled)
            return;
        fightingPlayer = player;
        player.BeginGoombaFight(this);
        // GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }

    private void ShooAway()
    {
        Vector2 playerPosition = fightingPlayer.GetComponent<Rigidbody2D>().position;
        retreatDirection = (movement.Position - playerPosition).normalized;
        if (retreatDirection == Vector2.zero)
            retreatDirection = Vector2.right;

        trashCarrier.DropNearPlayer(playerPosition, movement.NavigationBounds);
        // GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        retreatUntil = Time.time + Mathf.Max(0f, retreatDuration);
        movement.ClearRoute(retreatUntil);
        PlayerMovement player = fightingPlayer;
        EndFight();

        player.goombaShooed.Invoke();
        Shooed?.Invoke();
        GameManager.instance.RecordGoombaShooed();
    }

    public void EndFight()
    {
        if (fightingPlayer != null)
            fightingPlayer.EndGoombaFight(this);
        fightingPlayer = null;
    }

    public void ResetState()
    {
        EndFight();
        retreatUntil = 0f;
        retreatDirection = Vector2.zero;
    }
}
