using UnityEngine;

public class TrashGenerator : MonoBehaviour 
{
    public GameObject TrashPrefab;
    public Sprite[] TrashSprites;
    private void Start()
    {
        for(int i = 0; i < 5; i++)
        {
            SpawnTrash();
        }
        
    }
    // Spawn trash in random coordinate of 256x256 grid, randomise trash sprite
    private void SpawnTrash()
    {
        Vector3 TrashSpawnCoordinate = new Vector3(Random.Range(-128, 128), Random.Range(-128, 128), 0);
        GameObject NewTrash = Instantiate(TrashPrefab, TrashSpawnCoordinate, Quaternion.identity);
        var NewTrashSprite = TrashSprites[Random.Range(0, TrashSprites.Length)];
        NewTrash.transform.Find("trash").GetComponentInChildren<SpriteRenderer>().sprite = NewTrashSprite;
    }

}
