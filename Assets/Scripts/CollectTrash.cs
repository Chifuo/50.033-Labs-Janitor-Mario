using UnityEngine;

public class CollectTrash : MonoBehaviour
{ 
    public float Timer = 3;
    public bool CollideTrash = false;
    private void Update()
    {
        if (CollideTrash == true && Input.GetKey(KeyCode.F))
        {
            Timer -= 1 * Time.deltaTime;
            Debug.Log("Collecting trash");
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            Timer = 3;
        }
        if (CollideTrash == true && Timer <= 0)
        {
            Timer = 0;
            Destroy(gameObject);
            
            Debug.Log("Collected trash");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SpriteRenderer spriteRenderer = transform.Find("icon").GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.enabled = true;
            CollideTrash = true;
            //CollectingTrash.Add(other.gameObject);
            Debug.Log("Collided with trash");
            Debug.Log(CollideTrash);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SpriteRenderer spriteRenderer = transform.Find("icon").GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.enabled = false;
            CollideTrash = false;
            Debug.Log("No longer collide with trash");
            Debug.Log(CollideTrash);
        }
    }
}
