using System.Collections.Generic;
using UnityEngine;

public class ViewContent : GAction, IMultiTargetAction
{
    [SerializeField] private List<GameObject> _visitedLocations;
    [SerializeField] private int _targetLocations;

    public bool hasMultipleTargets => hasMultiLocationTarget;
    public int targetLocations => _targetLocations;
    public List<GameObject> visitedLocations => _visitedLocations;

    void Start()
    {
        _visitedLocations = new List<GameObject>();

        if (targetLocations == 0)
        {
            var queue = GWorld.Instance.GetQueue(locationResourceName);
            if (queue == null)
            {
                Debug.LogError("Content area queue not initialized.");
            }

            _targetLocations = Random.Range(5, Mathf.Min(10, queue.que.Count + 1));
            Debug.Log("Generated target locations: " + targetLocations);
        }
    }

    public GameObject GetNextTarget()
    {
        var queue = GWorld.Instance.GetQueue(locationResourceName);
        if (queue == null || queue.que.Count == 0)
        {
            Debug.LogWarning("Queue is empty or not initialized.");
            return null; // Tidak ada target yang tersedia
        }

        GameObject newTarget = null;

        while (queue.que.Count > 0)
        {
            // Ambil resource dari antrean
            newTarget = queue.RemoveResource();

            // Jika target belum dikunjungi, tambahkan ke visitedLocations
            if (!visitedLocations.Contains(newTarget))
            {
                visitedLocations.Add(newTarget);
                GWorld.Instance.GetWorld().ModifyState("FreeContentArea", -1);
                return newTarget;
            }

            // Kembalikan resource ke antrean jika sudah dikunjungi
            queue.AddResource(newTarget);
        }

        Debug.LogWarning("No unvisited targets available.");
        return null; // Tidak ada target yang belum dikunjungi
    }

    public override bool PrePerform()
    {
        target = GetNextTarget();
        if (target != null)
        {
            GWorld.Instance.GetQueue(locationResourceName).RemoveResource(target);
            return true;
        }

        Debug.Log("No unvisited content locations available.");
        return false;
    }

    public override bool PostPerform()
    {
        if (_visitedLocations.Count >= _targetLocations)
        {
            AddAreaResource();
            Debug.Log("All target locations visited.");
            beliefs.ModifyState("viewedContent", 1);
            return true;
        }

        return false;
    }

    public void AddAreaResource()
    {
        GWorld.Instance.GetQueue(locationResourceName).AddResource(target);
        GWorld.Instance.GetWorld().ModifyState("FreeContentArea", 1);
    }
}
