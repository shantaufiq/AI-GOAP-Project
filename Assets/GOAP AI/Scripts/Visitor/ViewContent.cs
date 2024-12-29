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
        foreach (var location in GWorld.Instance.GetQueue(locationResourceName).que)
        {
            if (!visitedLocations.Contains(location))
            {
                visitedLocations.Add(location);
                return location;
            }
        }
        return null;
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
            GWorld.Instance.GetQueue(locationResourceName).AddResource(target);
            Debug.Log("All target locations visited.");
            beliefs.ModifyState("viewedContent", 1);
            return true;
        }

        return false;
    }
}
