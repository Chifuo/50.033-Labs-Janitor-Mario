using UnityEngine;

public class TrashCollector : MonoBehaviour
{
    public Transform dropOffPoint;
    [Min(0.1f)] public float deliveryRadius = 3f;
    public GameManager gameManager;

    public Vector2 DropOffPosition => dropOffPoint != null ? (Vector2)dropOffPoint.position : (Vector2)transform.position;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    public bool TryReceive(CollectTrash trash, EnemyMovement carrier)
    {
        if (gameManager == null || gameManager.IsGameOver || trash == null || carrier == null ||
            Vector2.Distance(carrier.Position, DropOffPosition) > deliveryRadius)
            return false;

        if (!trash.TryDeliver(carrier))
            return false;

        gameManager.RecordGoombaDelivery();
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 position = DropOffPosition;
        Gizmos.DrawWireSphere(new Vector3(position.x, position.y, transform.position.z), deliveryRadius);
    }
}
