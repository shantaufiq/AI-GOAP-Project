using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] objectPrefabs;
    public Transform spawnPoint;
    public int maxObjects = 5;
    public float spawnInterval = 2f;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private int currentPrefabIndex = 0;

    void Start()
    {
        StartCoroutine(SpawnObjects());
    }

    IEnumerator SpawnObjects()
    {
        while (spawnedObjects.Count < maxObjects)
        {
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnObject()
    {
        float randomX = Random.Range(-2f, 2f);
        float randomZ = Random.Range(-2f, 2f);
        Vector3 spawnPosition = new Vector3(spawnPoint.position.x + randomX, spawnPoint.position.y, spawnPoint.position.z + randomZ);

        GameObject newObject = Instantiate(objectPrefabs[currentPrefabIndex], spawnPosition, Quaternion.identity);
        spawnedObjects.Add(newObject);

        currentPrefabIndex++;

        if (currentPrefabIndex >= objectPrefabs.Length)
        {
            currentPrefabIndex = 0;
        }
    }

    public void RemoveObjectFromList(GameObject obj)
    {
        if (spawnedObjects.Contains(obj))
        {
            spawnedObjects.Remove(obj);
            Destroy(obj);
        }

        if (spawnedObjects.Count < maxObjects)
        {
            StartCoroutine(SpawnObjects());
        }
    }
}
