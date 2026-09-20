using System.Collections.Generic;
using UnityEngine;

public class TrashGenerator : MonoBehaviour
{
    public GameObject TrashPrefab;
    public Sprite[] TrashSprites;
    [Min(5)] public int MinimumTrashCount = 5;

    private readonly List<GameObject> activeTrash = new List<GameObject>();
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        ReplenishTrash();
    }

    private void Update()
    {
        if (gameManager != null && gameManager.IsGameOver)
            return;
        activeTrash.RemoveAll(trash => trash == null);
        ReplenishTrash();
    }

    private void ReplenishTrash()
    {
        int groundCount = 0;
        foreach (GameObject trash in activeTrash)
        {
            if (trash != null && (!trash.TryGetComponent(out CollectTrash collectible) || collectible.IsOnGround))
                groundCount++;
        }
        while (groundCount < Mathf.Max(5, MinimumTrashCount))
        {
            SpawnTrash();
            groundCount++;
        }
    }

    // Spawn inside the 256 x 256 map with a random trash sprite.
    // If we implement coin multipliers, we could refactor this such that coins also spawn in a 256 x 256 map.
    private void SpawnTrash()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-128, 128), Random.Range(-128, 128), 0);
        GameObject newTrash = Instantiate(TrashPrefab, spawnPosition, Quaternion.identity);
        activeTrash.Add(newTrash);
        var sprite = TrashSprites[Random.Range(0, TrashSprites.Length)];
        newTrash.transform.Find("trash").GetComponentInChildren<SpriteRenderer>().sprite = sprite;
    }
}
