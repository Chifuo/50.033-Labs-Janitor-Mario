using UnityEngine;

public class GoombaTrashCarrier : MonoBehaviour
{
    [SerializeField, Min(0f)] private float carryHeight = 12f;
    [SerializeField, Min(0f)] private float droppedTrashProtection = 4f;
    private EnemyMovement movement;
    private CollectTrash carriedTrash;

    public float CarryHeight => carryHeight;
    public bool IsCarryingTrash => carriedTrash != null;
    public Vector2 Position => movement != null ? movement.Position : (Vector2)transform.position;
    public void Initialize(EnemyMovement owner) => movement = owner;

    public bool TryPickUp(CollectTrash trash)
    {
        if (carriedTrash != null || trash == null || !trash.TryPickUp(this))
            return false;
        carriedTrash = trash;
        return true;
    }

    public bool TryDeliver(TrashCollector collector)
    {
        if (collector == null || carriedTrash == null || !collector.TryReceive(carriedTrash, this))
            return false;
        carriedTrash = null;
        return true;
    }

    public void DropNearPlayer(Vector2 playerPosition, Rect bounds)
    {
        // Drop close for Mario to grab it.
        float dropRadius = 5f;
        Vector2 dropPosition = playerPosition + Random.insideUnitCircle * dropRadius;
        dropPosition.x = Mathf.Clamp(dropPosition.x, bounds.xMin, bounds.xMax);
        dropPosition.y = Mathf.Clamp(dropPosition.y, bounds.yMin, bounds.yMax);
        DropAt(dropPosition);
    }

    public void DropAt(Vector2 position)
    {
        if (carriedTrash != null)
            carriedTrash.Drop(this, position, droppedTrashProtection);
        carriedTrash = null;
    }

}
