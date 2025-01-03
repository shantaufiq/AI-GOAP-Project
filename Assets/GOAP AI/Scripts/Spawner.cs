using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject objectPrefab; // Prefab objek yang ingin di-spawn
    public Transform spawnPoint;    // Titik spawn objek
    public int maxObjects = 5;      // Batas maksimal objek yang di-spawn
    public float spawnInterval = 2f; // Waktu jeda antar spawn objek

    private List<GameObject> spawnedObjects = new List<GameObject>(); // List untuk menampung objek yang di-spawn

    void Start()
    {
        StartCoroutine(SpawnObjects());
    }

    // Coroutine untuk terus-menerus men-spawn objek dengan interval waktu tertentu
    IEnumerator SpawnObjects()
    {
        while (spawnedObjects.Count < maxObjects)
        {
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval); // Tunggu sesuai interval
        }
    }

    // Fungsi untuk men-spawn objek baru dengan posisi acak
    void SpawnObject()
    {
        // Menghasilkan posisi acak dalam rentang sumbu X dan Z
        float randomX = Random.Range(-2f, 2f);
        float randomZ = Random.Range(-2f, 2f);
        Vector3 spawnPosition = new Vector3(spawnPoint.position.x + randomX, spawnPoint.position.y, spawnPoint.position.z + randomZ);

        GameObject newObject = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);
        spawnedObjects.Add(newObject);
    }

    // Fungsi untuk menghapus objek dari list saat objek kembali ke titik spawn
    public void RemoveObjectFromList(GameObject obj)
    {
        if (spawnedObjects.Contains(obj))
        {
            spawnedObjects.Remove(obj);
            Destroy(obj); // Menghancurkan objek yang telah kembali ke spawn
        }
        // Setelah objek dihapus, periksa apakah kita perlu melanjutkan spawning
        if (spawnedObjects.Count < maxObjects)
        {
            StartCoroutine(SpawnObjects()); // Mulai ulang spawning jika objek kurang dari maxObjects
        }
    }
}
