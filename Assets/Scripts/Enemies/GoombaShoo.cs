using System;
using UnityEngine;
using UnityEngine.Events;

public class GoombaShoo : MonoBehaviour
{
    public UnityEvent onGoombaShooed;
    [SerializeField, Min(0f)] private float retreatSpeed = 5f;
    [SerializeField, Min(0f)] private float retreatDuration = 0.3f;
    [SerializeField] private IntVariable gameScore;
    private PlayerMovement fightingPlayer;
    private Vector2 retreatDirection;
    private float retreatUntil;
    private float activeRetreatSpeed;
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
        if (fightingPlayer != null && !fightingPlayer.isActiveAndEnabled)
            EndFight();
    }

    public void TryShoo(PlayerMovement player, float strength = 1f)
    {
        if (fightingPlayer != player || !CanBeShooed(player))
            return;

        ShooAway(player, strength);
    }

    public void TryAreaShoo(PlayerMovement player, float strength)
    {
        if (CanBeShooed(player))
            ShooAway(player, strength);
    }

    private bool CanBeShooed(PlayerMovement player)
    {
        return isActiveAndEnabled && player != null && player.isActiveAndEnabled &&
            movement != null && movement.isActiveAndEnabled && !IsRetreating && !gameScore.GameOver;
    }

    public Vector2 GetRetreatVelocity()
    {
        Vector2 step = retreatDirection * activeRetreatSpeed * Time.fixedDeltaTime;
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
        if (movement == null || !movement.isActiveAndEnabled || fightingPlayer != null ||
            IsRetreating || !other.CompareTag("Player"))
            return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null || !player.isActiveAndEnabled)
            return;
        fightingPlayer = player;
        player.BeginGoombaFight(this);
    }

    private void ShooAway(PlayerMovement player, float strength)
    {
        Vector2 playerPosition = player.GetComponent<Rigidbody2D>().position;
        retreatDirection = (movement.Position - playerPosition).normalized;
        if (retreatDirection == Vector2.zero)
            retreatDirection = Vector2.right;

        trashCarrier.DropNearPlayer(playerPosition, movement.NavigationBounds);
        float multiplier = Mathf.Max(1f, strength);
        activeRetreatSpeed = Mathf.Max(0f, retreatSpeed) * multiplier;
        retreatUntil = Time.time + Mathf.Max(0f, retreatDuration) * multiplier;
        movement.ClearRoute(retreatUntil);
        EndFight();

        Shooed?.Invoke();
        onGoombaShooed.Invoke();
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
        activeRetreatSpeed = 0f;
        retreatDirection = Vector2.zero;
    }
}
