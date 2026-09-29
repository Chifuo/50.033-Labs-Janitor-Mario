using System.Collections.Generic;
using UnityEngine;

public class GeneratorManager : MonoBehaviour
{
    public GameObject TrashPrefab;
    public GameObject TrashCollectorPrefab;
    public GameObject CoinPrefab;
    public GameObject collector;
    public Sprite[] TrashSprites;
    [Min(5)] public int MinimumTrashCount = 5;
    [Min(0f)] private float CoinInterval = 30f;
    private float CoinIntervalCountdown;
    private readonly List<GameObject> activeTrash = new List<GameObject>();
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        ReplenishTrash();
        MoveTrashCollector();
        CoinIntervalCountdown = CoinInterval;
        ReplenishCoin();
    }

    private void Update()
    {
        if (gameManager != null && gameManager.IsGameOver)
            return;
        activeTrash.RemoveAll(trash => trash == null);
        ReplenishTrash();
        MoveTrashCollector();
        ReplenishCoin();
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
    private void ReplenishCoin()
    {
        CoinIntervalCountdown = Mathf.Max(0f, CoinIntervalCountdown - Time.deltaTime);
        if (CoinIntervalCountdown <= 0f)
        {
            SpawnCoin();
            CoinIntervalCountdown = CoinInterval;
        }
    }
    private void MoveTrashCollector()
    {
        collector = GameObject.FindGameObjectWithTag("TrashCollector");
        if (collector != null)
        {
            return;
        }
        RelocateTrashCollector();
    }

    // Spawn inside the 256 x 256 map with a random trash sprite.
    private void SpawnTrash()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-128, 128), Random.Range(-128, 128), 0);
        GameObject newTrash = Instantiate(TrashPrefab, spawnPosition, Quaternion.identity);
        activeTrash.Add(newTrash);
        var sprite = TrashSprites[Random.Range(0, TrashSprites.Length)];
        newTrash.transform.Find("trash").GetComponentInChildren<SpriteRenderer>().sprite = sprite;
    }

    private void SpawnCoin()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-128, 128), Random.Range(-128, 128), 0);
        GameObject newCoin = Instantiate(CoinPrefab, spawnPosition, Quaternion.identity);
    }

    public void RelocateTrashCollector()
    {
        bool isVertical = Random.value > 0.5f;
        int rangeValue = Random.Range(-119, 119);
        int switchValue = (Random.value > 0.5f) ? -121 : 121;
        int xCoordinate = 0;
        int yCoordinate = -121;
        int zRotation = 0;
        if ( isVertical && switchValue == -121 )
        {
            xCoordinate = switchValue; yCoordinate = rangeValue; zRotation = 270;
        }
        else if ( isVertical && switchValue == 121)
        {
            xCoordinate = switchValue; yCoordinate = rangeValue; zRotation = 90;
        }
        else if (!isVertical && switchValue == -121)
        {
            xCoordinate = rangeValue; yCoordinate = switchValue; zRotation = 0;
        }
        else if (!isVertical && switchValue == 121)
        {
            xCoordinate = rangeValue; yCoordinate = switchValue; zRotation = 180;
        }
        Vector3 spawnPosition = new Vector3(xCoordinate, yCoordinate, 0);
        if (collector != null)
        {
            // Ensure a pickup always changes the position, even if the random choice repeats.
            if (collector.transform.position == spawnPosition)
            {
                if (isVertical)
                    spawnPosition.y += 1f;
                else
                    spawnPosition.x += 1f;
            }
            collector.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0, 0, zRotation));
        }
        else
        {
            collector = Instantiate(TrashCollectorPrefab, spawnPosition, Quaternion.Euler(0, 0, zRotation));
        }
    }
}
