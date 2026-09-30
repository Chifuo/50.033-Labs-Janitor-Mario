using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private GoombaAvoidance avoidance;
    [SerializeField] private GoombaShoo shoo;
    [SerializeField] private GoombaVisuals visuals;
    [SerializeField] private GoombaTrashCarrier trashCarrier;

    private Rigidbody2D enemyBody;
    private BoxCollider2D enemyCollider;
    private GameManager gameManager;
    private bool initialized;

    public Vector2 Position => enemyBody != null ? enemyBody.position : (Vector2)transform.position;
    public Rect NavigationBounds => navigationBounds;
    public Vector2 BodySize => bodySize;
    public Vector2 ColliderOffset => colliderOffset;
    public BoxCollider2D BodyCollider => enemyCollider;
    public bool IsGameOver => gameManager != null && gameManager.IsGameOver;
    public bool IsCarryingTrash => trashCarrier != null && trashCarrier.IsCarryingTrash;
    public bool HasActiveRoute => initialized && isActiveAndEnabled && moveSpeed > 0f &&
        collector != null && waypointIndex < path.Count &&
        (IsCarryingTrash || (target != null && target.CanEnemyPickUp)) && !shoo.IsRetreating;

    private void Awake()
    {
        enemyBody = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<BoxCollider2D>();
        gameManager = FindFirstObjectByType<GameManager>();

        if (avoidance == null || shoo == null || visuals == null || trashCarrier == null)
        {
            enabled = false;
            return;
        }

        avoidance.Initialize(this);
        trashCarrier.Initialize(this);
        shoo.Initialize(this, trashCarrier);
        visuals.Initialize(enemyBody, shoo);
        initialized = true;
    }

    private void Start()
    {
        // offset a bit for collider and walls 
        bodySize = (Vector2)enemyCollider.bounds.size + Vector2.one * 0.2f;
        colliderOffset = (Vector2)enemyCollider.bounds.center - enemyBody.position;
        pathfinder = new GridPathfinder2D(navigationBounds, cellSize, CanTravel);
    }

    private void Update()
    {
        collector = FindFirstObjectByType<TrashCollector>();
        if (IsGameOver)
        {
            shoo.EndFight();
            return;
        }

        shoo.ProcessInput();
        // Uncomment to stop planning while touching Mario.
        // if (shoo.IsInteracting)
        //     return;
        if (collector == null || shoo.IsRetreating)
            return;

        bool targetLost = !IsCarryingTrash && (target == null || !target.CanEnemyPickUp) && path.Count > 0;
        if (targetLost || Time.time >= nextPathTime)
        {
            nextPathTime = Time.time + Mathf.Max(0.1f, repathInterval);
            FindRoute();
        }
    }

    private void FixedUpdate()
    {
        enemyBody.linearVelocity = Vector2.zero;
        if (IsGameOver)
            return;
        // Uncomment to stop moving while touching Mario.
        // if (shoo.IsInteracting)
        //     return;
        if (shoo.IsRetreating)
        {
            enemyBody.linearVelocity = shoo.GetRetreatVelocity();
            return;
        }
        if (collector == null ||
            (!IsCarryingTrash && (target == null || !target.CanEnemyPickUp)) || waypointIndex >= path.Count)
            return;

        Vector2 position = enemyBody.position;
        float arrivalDistance = IsCarryingTrash ? collector.deliveryRadius : stoppingDistance;
        // Only stop near the final waypoint
        if (waypointIndex == path.Count - 1 &&
            Vector2.Distance(position, path[waypointIndex]) <= arrivalDistance && CanTravel(position, path[waypointIndex]))
        {
            CompleteArrival();
            return;
        }

        while (waypointIndex < path.Count && Vector2.Distance(position, path[waypointIndex]) < 0.1f)
            waypointIndex++;
        if (waypointIndex >= path.Count)
        {
            CompleteArrival();
            return;
        }

        Vector2 delta = path[waypointIndex] - position;
        Vector2 velocity = Vector2.ClampMagnitude(delta / Time.fixedDeltaTime, Mathf.Max(0f, moveSpeed));
        enemyBody.linearVelocity = avoidance.AvoidGoombas(velocity, delta.magnitude);
        visuals.UpdateFacing(enemyBody.linearVelocity);
    }

    // Reset everything here when the Goomba is disabled.
    private void OnDisable()
    {
        if (enemyBody != null)
            enemyBody.linearVelocity = Vector2.zero;
        if (!initialized)
            return;

        avoidance.ResetAvoidance();
        shoo.ResetState();
        trashCarrier.DropAt(Position);
        visuals.ResetFeedback();
        ClearRoute();
    }

    // Pathfinding
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0.1f)] private float stoppingDistance = 2f;
    [SerializeField, Min(0.1f)] private float repathInterval = 0.75f;
    [SerializeField, Min(1f)] private float cellSize = 8f;
    [SerializeField] private Rect navigationBounds = new Rect(-128f, -128f, 256f, 256f);
    [SerializeField] private TrashCollector collector;
    private GridPathfinder2D pathfinder;
    private CollectTrash target;
    private readonly List<Vector2> path = new List<Vector2>();
    private int waypointIndex;
    private float nextPathTime;

    public void RequestRepath() => nextPathTime = 0f;
    public void DeferRepathUntil(float time) => nextPathTime = Mathf.Max(nextPathTime, time);

    public void ClearRoute(float resumeAt = 0f)
    {
        target = null;
        path.Clear();
        waypointIndex = 0;
        nextPathTime = resumeAt;
    }

    private void FindRoute()
    {
        waypointIndex = 0;
        if (IsCarryingTrash)
        {
            PlanRoute(collector.DropOffPosition, collector.deliveryRadius);
            return;
        }

        // Keep the same target if we can still reach it.
        if (target != null && target.CanEnemyPickUp && PlanRoute(target.transform.position, stoppingDistance))
            return;

        target = null;
        path.Clear();
        CollectTrash[] candidates = FindObjectsByType<CollectTrash>(FindObjectsSortMode.None);
        Array.Sort(candidates, (a, b) =>
            ((Vector2)a.transform.position - enemyBody.position).sqrMagnitude.CompareTo(
                ((Vector2)b.transform.position - enemyBody.position).sqrMagnitude));

        foreach (CollectTrash candidate in candidates)
        {
            if (candidate.CanEnemyPickUp && PlanRoute(candidate.transform.position, stoppingDistance))
            {
                target = candidate;
                return;
            }
        }
    }

    private bool PlanRoute(Vector2 destination, float arrivalDistance)
    {
        if (Vector2.Distance(enemyBody.position, destination) <= arrivalDistance && CanTravel(enemyBody.position, destination))
        {
            path.Clear();
            path.Add(destination);
            return true;
        }
        bool found = pathfinder.FindPath(enemyBody.position, destination, path);
        // Skip the first node if it would send us backwards.
        if (found && path.Count > 1 && CanTravel(enemyBody.position, path[1]))
            path.RemoveAt(0);
        return found;
    }

    private void CompleteArrival()
    {
        bool completed = IsCarryingTrash
            ? trashCarrier.TryDeliver(collector)
            : trashCarrier.TryPickUp(target);
        if (!completed)
            return;

        ClearRoute();
        avoidance.ResetAvoidance();
    }

    // Wall and map checks
    [SerializeField] private LayerMask obstacleLayers = ~0;
    private Vector2 bodySize;
    private Vector2 colliderOffset;

    public bool CanTravel(Vector2 from, Vector2 to)
    {
        if (!FitsInsideMap(from) || !FitsInsideMap(to))
            return false;

        // Check the full collider so we do not clip wall corners.
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(to + colliderOffset, bodySize, 0f, obstacleLayers))
        {
            if (IsObstacle(hit))
                return false;
        }
        Vector2 delta = to - from;
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(from + colliderOffset, bodySize,
            0f, delta.normalized, delta.magnitude, obstacleLayers))
        {
            if (IsObstacle(hit.collider))
                return false;
        }
        return true;
    }

    private bool FitsInsideMap(Vector2 position)
    {
        Vector2 center = position + colliderOffset;
        Vector2 halfSize = bodySize * 0.5f;
        return center.x - halfSize.x >= navigationBounds.xMin &&
            center.x + halfSize.x <= navigationBounds.xMax &&
            center.y - halfSize.y >= navigationBounds.yMin &&
            center.y + halfSize.y <= navigationBounds.yMax;
    }

    private bool IsObstacle(Collider2D collider)
    {
        return collider != null && collider != enemyCollider && !collider.isTrigger &&
            (collider.attachedRigidbody == null || collider.attachedRigidbody.bodyType == RigidbodyType2D.Static);
    }

    // Draw the current path
    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireCube(new Vector3(navigationBounds.center.x, navigationBounds.center.y, transform.position.z),
    //         new Vector3(navigationBounds.width, navigationBounds.height, 0f));
    //     Gizmos.color = Color.cyan;
    //     Vector3 previous = transform.position;
    //     for (int i = waypointIndex; i < path.Count; i++)
    //     {
    //         Vector3 next = new Vector3(path[i].x, path[i].y, transform.position.z);
    //         Gizmos.DrawLine(previous, next);
    //         previous = next;
    //     }
    // }
}
