using System.Collections.Generic;
using UnityEngine;

public class CollectTrash : MonoBehaviour
{
    [Min(0f)] public float collectionDuration = 0.5f;
    public float RemainingTime => collectionTimer;
    public bool CanPlayerCollect { get; private set; }
    public bool IsOnGround => isActiveAndEnabled && !isCarried && !consumed;
    public bool CanEnemyPickUp => IsOnGround && Time.time >= enemyPickupAllowedAt;
    private readonly HashSet<Collider2D> nearbyPlayers = new HashSet<Collider2D>();
    private Collider2D pickupCollider;
    private GameManager gameManager;
    private EnemyMovement carrier;
    private Transform groundParent;
    private bool isCarried;
    private bool consumed;
    private float enemyPickupAllowedAt;
    private float collectionTimer;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider2D>();
        gameManager = FindFirstObjectByType<GameManager>();
        collectionTimer = collectionDuration;
    }

    private void Update()
    {
        nearbyPlayers.RemoveWhere(player => player == null || !player.enabled || !player.gameObject.activeInHierarchy);
        CanPlayerCollect = IsOnGround && nearbyPlayers.Count > 0 &&
            (gameManager == null || !gameManager.IsGameOver) && !PlayerIsFighting();

        if (!CanPlayerCollect || !Input.GetKey(KeyCode.F))
        {
            collectionTimer = collectionDuration;
            return;
        }

        collectionTimer = Mathf.Max(0f, collectionTimer - Time.deltaTime);
        if (collectionTimer <= 0f && gameManager != null)
        {
            Consume();
            gameManager.IncreaseScore(1);
        }
    }

    private bool PlayerIsFighting()
    {
        foreach (Collider2D player in nearbyPlayers)
        {
            PlayerMovement movement = player.GetComponentInParent<PlayerMovement>();
            if (movement != null && movement.IsFightingGoomba)
                return true;
        }
        return false;
    }

    private void LateUpdate()
    {
        if (isCarried && carrier != null)
            transform.position = carrier.transform.position + new Vector3(0f, carrier.carryHeight, -0.1f);
    }

    public bool TryPickUp(EnemyMovement enemy)
    {
        if (enemy == null || !CanEnemyPickUp || (gameManager != null && gameManager.IsGameOver))
            return false;

        carrier = enemy;
        isCarried = true;
        groundParent = transform.parent;
        transform.SetParent(enemy.transform, true);
        if (pickupCollider != null)
            pickupCollider.enabled = false;
        ResetPlayerInteraction();
        return true;
    }

    public bool Drop(EnemyMovement enemy, Vector3 position, float protectionDuration)
    {
        if (!isCarried || carrier != enemy || consumed)
            return false;

        transform.SetParent(groundParent, true);
        transform.position = new Vector3(position.x, position.y, 0f);
        carrier = null;
        isCarried = false;
        enemyPickupAllowedAt = Time.time + Mathf.Max(0f, protectionDuration);
        ResetPlayerInteraction();
        if (pickupCollider != null)
            pickupCollider.enabled = true;
        return true;
    }

    public bool TryDeliver(EnemyMovement enemy)
    {
        if (!isCarried || carrier != enemy || consumed)
            return false;

        Consume();
        return true;
    }

    private void Consume()
    {
        // mark this item immediately to prevent double collection.
        consumed = true;
        if (pickupCollider != null)
            pickupCollider.enabled = false;
        ResetPlayerInteraction();
        Destroy(gameObject);
    }

    private void ResetPlayerInteraction()
    {
        nearbyPlayers.Clear();
        CanPlayerCollect = false;
        collectionTimer = collectionDuration;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrackPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // handles a dropped item reappearing underneath Mario.
        TrackPlayer(other);
    }

    private void TrackPlayer(Collider2D other)
    {
        if (IsOnGround && other.CompareTag("Player"))
        {
            nearbyPlayers.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        nearbyPlayers.Remove(other);
        if (nearbyPlayers.Count == 0)
            ResetPlayerInteraction();
    }
}
