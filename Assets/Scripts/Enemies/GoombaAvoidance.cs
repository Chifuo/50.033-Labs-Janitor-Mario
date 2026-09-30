using UnityEngine;

public class GoombaAvoidance : MonoBehaviour
{
    private EnemyMovement movement;

    public void Initialize(EnemyMovement owner) => movement = owner;
    public void ResetAvoidance() => avoidanceUntil = 0f;

    // Decide who goes first when Goombas touch each other
    [SerializeField, Min(0.05f)] private float avoidanceLookAhead = 0.3f;
    private static readonly float[] avoidanceAngles = { -30f, -60f, -90f, 30f, 60f, 90f };
    private Vector2 avoidanceDirection;
    private float avoidanceUntil;

    public Vector2 AvoidGoombas(Vector2 desiredVelocity, float waypointDistance)
    {
        float speed = desiredVelocity.magnitude;
        if (speed <= 0f)
            return Vector2.zero;

        // Check past the next waypoint
        float lookAhead = speed * Mathf.Max(Time.fixedDeltaTime, avoidanceLookAhead);
        Vector2 forward = desiredVelocity / speed;
        EnemyMovement blocker = FindBlockingGoomba(forward, lookAhead);

        if (blocker == null)
        {
            // Pick a new route after going around the other Goomba
            if (avoidanceUntil > 0f)
                movement.RequestRepath();
            avoidanceUntil = 0f;
            return movement.CanTravel(movement.Position, movement.Position + forward * Mathf.Min(waypointDistance, lookAhead))
                ? desiredVelocity : Vector2.zero;
        }

        // Lower ID goes first, unless that Goomba is standing still
        if (movement.GetInstanceID() > blocker.GetInstanceID() && blocker.HasActiveRoute)
        {
            avoidanceUntil = 0f;
            return Vector2.zero;
        }

        // Stick with the sidestep to avoid jitter
        // if (Time.time < avoidanceUntil && CanAvoidTowards(avoidanceDirection, lookAhead))
        //     return avoidanceDirection * speed;
        if (Time.time < avoidanceUntil && CanAvoidTowards(avoidanceDirection, lookAhead))
            return avoidanceDirection * speed;

        // Vector2 forward = desiredVelocity / speed;
        // if (CanAvoidTowards(forward, lookAhead))
        //     return desiredVelocity;

        foreach (float angle in avoidanceAngles)
        {
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * forward;
            if (!CanAvoidTowards(direction, lookAhead))
                continue;

            avoidanceDirection = direction;
            avoidanceUntil = Time.time + 0.25f;
            // Finish the sidestep before replanning
            movement.DeferRepathUntil(avoidanceUntil);
            return direction * speed;
        }

        // Wait if neither side has space
        avoidanceUntil = 0f;
        return Vector2.zero;
    }

    private bool CanAvoidTowards(Vector2 direction, float distance)
    {
        if (!movement.CanTravel(movement.Position, movement.Position + direction * distance))
            return false;

        return FindBlockingGoomba(direction, distance) == null;
    }

    private EnemyMovement FindBlockingGoomba(Vector2 direction, float distance)
    {
        EnemyMovement nearest = null;
        float nearestDistance = float.PositiveInfinity;
        // check for goombas ahead
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(movement.Position + movement.ColliderOffset,
            movement.BodySize, 0f, direction, distance))
        {
            if (hit.collider == null || hit.collider == movement.BodyCollider || hit.collider.isTrigger)
                continue;
            EnemyMovement other = hit.collider.GetComponentInParent<EnemyMovement>();
            if (other == null || other == movement)
                continue;

            // Allow sliding past when already touching each other.
            Vector2 away = (Vector2)movement.BodyCollider.bounds.center - (Vector2)hit.collider.bounds.center;
            // if (hit.distance <= 0f && Vector2.Dot(direction, away) > 0f)
            if (hit.distance <= 0f && Vector2.Dot(direction, away.normalized) >= -0.001f)
                continue;
            // Nearest blocker first,  use ID to break a tie.
            if (hit.distance < nearestDistance ||
                (hit.distance == nearestDistance && nearest != null &&
                 other.GetInstanceID() < nearest.GetInstanceID()))
            {
                nearest = other;
                nearestDistance = hit.distance;
            }
        }
        return nearest;
    }
}
