using UnityEngine;
using UnityEngine.Events;

public class TrashCollector : MonoBehaviour
{
    public UnityEvent onTrashDelivered;
    public Transform dropOffPoint;
    [SerializeField] private IntVariable gameScore;
    [Min(0.1f)] public float deliveryRadius = 3f;

    public Vector2 DropOffPosition => dropOffPoint != null ? (Vector2)dropOffPoint.position : (Vector2)transform.position;

    private void Awake()
    {
    }
    public bool TryReceive(CollectTrash trash, GoombaTrashCarrier carrier)
    {
        if (gameScore.GameOver || trash == null || carrier == null ||
            Vector2.Distance(carrier.Position, DropOffPosition) > deliveryRadius)
            return false;

        if (!trash.TryDeliver(carrier))
            return false;

        onTrashDelivered.Invoke();
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 position = DropOffPosition;
        Gizmos.DrawWireSphere(new Vector3(position.x, position.y, transform.position.z), deliveryRadius);
    }
}
