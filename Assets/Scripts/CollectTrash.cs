using UnityEngine;

public class CollectTrash : MonoBehaviour
{
    // Upon touching collision area of trash, icon appears
    // Player hold F for 3 second to 'pick up' trash
    public float Timer = 3;
    public bool CollideTrash = false;
    private void Update()
    {
        if (CollideTrash == true && Input.GetKey(KeyCode.F))
        {
            Timer -= 1 * Time.deltaTime;
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            Timer = 3;
        }
        if (CollideTrash == true && Timer <= 0)
        {
            Timer = 0;
            Destroy(gameObject);
            FindObjectOfType<GameManager>().IncreaseScore(1);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SpriteRenderer spriteRenderer = transform.Find("icon").GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.enabled = true;
            CollideTrash = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SpriteRenderer spriteRenderer = transform.Find("icon").GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.enabled = false;
            CollideTrash = false;
        }
    }
}
