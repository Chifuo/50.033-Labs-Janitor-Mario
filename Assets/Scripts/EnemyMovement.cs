using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyMovement : MonoBehaviour
{

    // Shared components 
    private Rigidbody2D enemyBody;
    private BoxCollider2D enemyCollider;
    private GameManager gameManager;
    public Vector2 Position => enemyBody != null ? enemyBody.position : (Vector2)transform.position;

    private void Start()
    {
        enemyBody = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<BoxCollider2D>();
        enemySprite = GetComponentInChildren<SpriteRenderer>();
        if (goombaAnimator == null)
            goombaAnimator = GetComponentInChildren<Animator>();

        bodySize = (Vector2)enemyCollider.bounds.size + Vector2.one * 0.2f;
        colliderOffset = (Vector2)enemyCollider.bounds.center - enemyBody.position;
        pathfinder = new GridPathfinder2D(navigationBounds, cellSize, CanTravel);
        gameManager = FindFirstObjectByType<GameManager>();
        if (visual != null)
            visualRestPosition = visual.localPosition;
    }

    private void Update()
    {
        collector = FindFirstObjectByType<TrashCollector>();
        if (gameManager != null && gameManager.IsGameOver)
        {
            EndFight();
            return;
        }
        if (fightingPlayer != null)
        {
            if (!fightingPlayer.isActiveAndEnabled)
            {
                EndFight();
                // return; // Keep planning movement after the interaction ends uncomment to make it such that mario and goomba velocity = 0 upon contact.
            }
            else if (Input.GetKeyDown(shooKey))
                ShooAway();
            // return; // Contact allows shooing without pausing route planning, // uncomment to make it such that mario and goomba velocity = 0 upon contact.
        }
        if (collector == null || Time.time < retreatUntil)
            return;

        bool targetLost = carriedTrash == null && (target == null || !target.CanEnemyPickUp) && path.Count > 0;
        if (targetLost || Time.time >= nextPathTime)
        {
            nextPathTime = Time.time + Mathf.Max(0.1f, repathInterval);
            FindRoute();
        }
    }

    private void FixedUpdate()
    {
        enemyBody.linearVelocity = Vector2.zero;
        if (gameManager != null && gameManager.IsGameOver)
            return;
        // Keep following the route while touching Mario
        // uncomment to make it such that mario and goomba velocity = 0 upon contact.
        // if (fightingPlayer != null)
        //     return;
        if (Time.time < retreatUntil)
        {
            Vector2 step = retreatDirection * Mathf.Max(0f, retreatSpeed) * Time.fixedDeltaTime;
            if (CanTravel(Position, Position + step))
                enemyBody.linearVelocity = step / Time.fixedDeltaTime;
            return;
        }
        if (collector == null ||
            (carriedTrash == null && (target == null || !target.CanEnemyPickUp)) || waypointIndex >= path.Count)
            return;

        Vector2 position = enemyBody.position;
        float arrivalDistance = carriedTrash != null ? collector.deliveryRadius : stoppingDistance;
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
        enemyBody.linearVelocity = AvoidGoombas(velocity, delta.magnitude);
        if (enemySprite != null && Mathf.Abs(enemyBody.linearVelocity.x) > 0.01f)
            enemySprite.flipX = enemyBody.linearVelocity.x > 0f;
    }

    private void OnDisable()
    {
        avoidanceUntil = 0f;
        EndFight();
        if (carriedTrash != null)
            carriedTrash.Drop(this, Position, droppedTrashProtection);
        carriedTrash = null;
        if (enemyBody != null)
            enemyBody.linearVelocity = Vector2.zero;
        target = null;
        path.Clear();
        nextPathTime = 0f;

        CancelInvoke(nameof(HideAlert));
        HideAlert();

        if (hopRoutine != null)
        {
            StopCoroutine(hopRoutine);
            hopRoutine = null;

            if (visual != null)
                visual.localPosition = visualRestPosition;
        }
    }

    [Min(0f)] public float moveSpeed = 5f;
    [Min(0.1f)] public float stoppingDistance = 2f;
    [Min(0.1f)] public float repathInterval = 0.75f;
    [Min(1f)] public float cellSize = 8f;
    public Rect navigationBounds = new Rect(-128f, -128f, 256f, 256f);
    private GridPathfinder2D pathfinder;
    private CollectTrash target;
    private readonly List<Vector2> path = new List<Vector2>();
    private int waypointIndex;
    private float nextPathTime;

    private void FindRoute()
    {
        waypointIndex = 0;
        if (carriedTrash != null)
        {
            PlanRoute(collector.DropOffPosition, collector.deliveryRadius);
            return;
        }

        // Keep pursuing trash until it disappears or becomes unreachable.
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
        // Replanning mid-edge shouldnt send Goomba backwards to its starting grid node.
        if (found && path.Count > 1 && CanTravel(enemyBody.position, path[1]))
            path.RemoveAt(0);
        return found;
    }

    // Trash pickup and delivery
    public TrashCollector collector;
    [Min(0f)] public float carryHeight = 12f;
    private CollectTrash carriedTrash;
    public bool IsCarryingTrash => carriedTrash != null;

    private void CompleteArrival()
    {
        if (carriedTrash != null)
        {
            if (!collector.TryReceive(carriedTrash, this))
                return;
            carriedTrash = null;
        }
        else
        {
            if (target == null || !target.TryPickUp(this))
                return;
            carriedTrash = target;
        }

        target = null;
        path.Clear();
        waypointIndex = 0;
        nextPathTime = 0f;
        avoidanceUntil = 0f;
    }

    // Step right when another Goomba blocks the route.
    [Min(0.05f)] public float avoidanceLookAhead = 0.3f;
    private static readonly float[] avoidanceAngles = { -30f, -60f, -90f, 30f, 60f, 90f };
    private Vector2 avoidanceDirection;
    private float avoidanceUntil;

    private Vector2 AvoidGoombas(Vector2 desiredVelocity, float waypointDistance)
    {
        float speed = desiredVelocity.magnitude;
        if (speed <= 0f)
            return Vector2.zero;

        float lookAhead = Mathf.Min(waypointDistance,
            speed * Mathf.Max(Time.fixedDeltaTime, avoidanceLookAhead));
        // Hold a sidestep briefly instead of switching left/right every physics tick.
        if (Time.time < avoidanceUntil && CanAvoidTowards(avoidanceDirection, lookAhead))
            return avoidanceDirection * speed;

        Vector2 forward = desiredVelocity / speed;
        if (CanAvoidTowards(forward, lookAhead))
            return desiredVelocity;

        foreach (float angle in avoidanceAngles)
        {
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * forward;
            if (!CanAvoidTowards(direction, lookAhead))
                continue;

            avoidanceDirection = direction;
            avoidanceUntil = Time.time + 0.25f;
            return direction * speed;
        }

        // Wait if neither side has space
        avoidanceUntil = 0f;
        return Vector2.zero;
    }

    private bool CanAvoidTowards(Vector2 direction, float distance)
    {
        if (!CanTravel(Position, Position + direction * distance))
            return false;

        // Check all layers so avoidance also works when Goombas aren't in obstacleLayers.
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(Position + colliderOffset,
            bodySize, 0f, direction, distance))
        {
            if (hit.collider == null || hit.collider == enemyCollider || hit.collider.isTrigger)
                continue;
            EnemyMovement other = hit.collider.GetComponentInParent<EnemyMovement>();
            if (other == null || other == this)
                continue;

            // If already touching, allow a step away instead of getting stuck together or move together
            Vector2 away = (Vector2)enemyCollider.bounds.center - (Vector2)hit.collider.bounds.center;
            if (hit.distance <= 0f && Vector2.Dot(direction, away) > 0f)
                continue;
            return false;
        }
        return true;
    }

    // Navigation collision checks
    public LayerMask obstacleLayers = ~0;
    private Vector2 bodySize;
    private Vector2 colliderOffset;

    private bool CanTravel(Vector2 from, Vector2 to)
    {
        if (!FitsInsideMap(from) || !FitsInsideMap(to))
            return false;

        // Check Goomba's whole body so a route cannot cut through wall
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

    // Getting shoo'ed retreat when Space is pressed.
    public KeyCode shooKey = KeyCode.Space;
    [Min(0f)] public float retreatSpeed = 5f;
    [Min(0f)] public float retreatDuration = 0.3f;
    [Min(0f)] public float droppedTrashProtection = 4f;
    private PlayerMovement fightingPlayer;
    private Vector2 retreatDirection;
    private float retreatUntil;

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
        // Release Mario's interaction lock when the Goomba moves away.
        if (fightingPlayer != null &&
            collision.collider.GetComponentInParent<PlayerMovement>() == fightingPlayer)
            EndFight();
    }

    private void InteractWithPlayer(Collider2D other)
    {
        if (fightingPlayer != null || Time.time < retreatUntil || !other.CompareTag("Player") ||
            (gameManager != null && gameManager.IsGameOver))
            return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null || !player.isActiveAndEnabled)
            return;
        fightingPlayer = player;
        player.BeginGoombaFight(this);
        //enemyBody.linearVelocity = Vector2.zero;
    }

    private void ShooAway()
    {
        Vector2 playerPosition = fightingPlayer.GetComponent<Rigidbody2D>().position;
        retreatDirection = (Position - playerPosition).normalized;
        if (retreatDirection == Vector2.zero)
            retreatDirection = Vector2.right;

        // Drop near Mario so he has a chance to steal the trash.
        float dropRadius = 5f;
        Vector2 dropPosition =
            playerPosition + UnityEngine.Random.insideUnitCircle * dropRadius;
        dropPosition.x = Mathf.Clamp(dropPosition.x, navigationBounds.xMin, navigationBounds.xMax);
        dropPosition.y = Mathf.Clamp(dropPosition.y, navigationBounds.yMin, navigationBounds.yMax);
        if (carriedTrash != null)
            carriedTrash.Drop(this, dropPosition, droppedTrashProtection);
        carriedTrash = null;
        target = null;
        path.Clear();
        // enemyBody.linearVelocity = Vector2.zero;
        retreatUntil = Time.time + Mathf.Max(0f, retreatDuration);
        nextPathTime = retreatUntil;
        fightingPlayer.goombaShooed.Invoke();
        EndFight();

        // Show feedback only after a successful shoo
        if (alert != null)
        {
            alert.SetActive(true);
            CancelInvoke(nameof(HideAlert));
            Invoke(nameof(HideAlert), 1f);
        }

        if (goombaAnimator != null)
            goombaAnimator.SetTrigger("onShoo");

        if (visual != null)
        {
            if (hopRoutine != null)
                StopCoroutine(hopRoutine);

            hopRoutine = StartCoroutine(Hop());
        }
    }

    private void EndFight()
    {
        if (fightingPlayer != null)
            fightingPlayer.EndGoombaFight(this);
        fightingPlayer = null;
    }

    // Walking animation, the shoo alert, and a hop of the visual child.
    public Animator goombaAnimator;
    private SpriteRenderer enemySprite;
    public GameObject alert;
    public Transform visual;
    public float hopHeight = 0.6f;
    public float hopDuration = 0.3f;
    private Vector3 visualRestPosition;
    private Coroutine hopRoutine;

    private void LateUpdate()
    {
        // Use total speed 
        if (goombaAnimator != null && enemyBody != null)
            goombaAnimator.SetFloat("xSpeed", enemyBody.linearVelocity.magnitude);
    }

    private void HideAlert()
    {
        if (alert != null)
            alert.SetActive(false);
    }

    private IEnumerator Hop()
    {
        float duration = Mathf.Max(0.01f, hopDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            // A half sine wave lifts the visual and returns it to its resting height.
            float height = Mathf.Sin(progress * Mathf.PI) * hopHeight;

            visual.localPosition =
                visualRestPosition + Vector3.up * height;

            elapsed += Time.deltaTime;
            yield return null;
        }

        visual.localPosition = visualRestPosition;
        hopRoutine = null;
    }

    // Optional scene debugging; leave disabled during normal gameplay.
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
