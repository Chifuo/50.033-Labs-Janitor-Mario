using UnityEngine;

public class TrashCollector : MonoBehaviour
{
    public Transform dropOffPoint;
    [Min(0.1f)] public float deliveryRadius = 3f;

    public Vector2 DropOffPosition => dropOffPoint != null ? (Vector2)dropOffPoint.position : (Vector2)transform.position;

    private void Awake()
    {
    }

    public bool TryReceive(CollectTrash trash, GoombaTrashCarrier carrier)
    {
        if (GameManager.instance == null || GameManager.instance.IsGameOver || trash == null || carrier == null ||
            Vector2.Distance(carrier.Position, DropOffPosition) > deliveryRadius)
            return false;

        if (!trash.TryDeliver(carrier))
            return false;

        GameManager.instance.RecordGoombaDelivery();
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 position = DropOffPosition;
        Gizmos.DrawWireSphere(new Vector3(position.x, position.y, transform.position.z), deliveryRadius);
    }
}
