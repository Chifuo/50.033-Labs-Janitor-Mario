using System.Collections.Generic;
using UnityEngine;

public class TrashGenerator : MonoBehaviour
{
    public GameObject TrashPrefab;
    public Sprite[] TrashSprites;
    [Min(5)] public int MinimumTrashCount = 5;

    private readonly List<GameObject> activeTrash = new List<GameObject>();

    private void Start()
    { ReplenishTrash(); }


    private void Update()
    {
        activeTrash.RemoveAll(trash => trash == null);
        ReplenishTrash();
    }

    private void ReplenishTrash()
    {
        while (activeTrash.Count < Mathf.Max(5, MinimumTrashCount))
        { SpawnTrash(); }
    }
    // Spawn trash in random coordinate of 256x256 grid, randomise trash sprite
    private void SpawnTrash()
    {
        Vector3 TrashSpawnCoordinate = new Vector3(Random.Range(-128, 128), Random.Range(-128, 128), 0);
        GameObject NewTrash = Instantiate(TrashPrefab, TrashSpawnCoordinate, Quaternion.identity);
        activeTrash.Add(NewTrash);
        var NewTrashSprite = TrashSprites[Random.Range(0, TrashSprites.Length)];
        NewTrash.transform.Find("trash").GetComponentInChildren<SpriteRenderer>().sprite = NewTrashSprite;
    }

}
