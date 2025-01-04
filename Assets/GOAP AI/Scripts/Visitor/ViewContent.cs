using System.Collections.Generic;
using UnityEngine;

public class ViewContent : GAction, IMultiTargetAction
{
    [SerializeField] private List<GameObject> _visitedLocations;
    [SerializeField] private int _targetLocations;

    public bool hasMultipleTargets => hasMultiLocationTarget;
    public int targetLocations => _targetLocations;
    public List<GameObject> visitedLocations => _visitedLocations;

    GameObject resource;

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
        resource = GWorld.Instance.GetQueue(locationResourceName).RemoveResource();
        if (resource != null)
        {
            GWorld.Instance.GetWorld().ModifyState("FreeContentArea", -1);
            return resource;
        }

        return null;
    }

    public override bool PrePerform()
    {
        if (beliefs.HasState("skipIntroArea"))
        {
            Debug.Log("Skipping directly to view content.");
            beliefs.RemoveState("skipIntroArea");
            beliefs.RemoveState("atIntroArea");
        }

        return true;
    }

    public override bool PostPerform()
    {

        Debug.Log($"{this.gameObject.name} has visited all content.....!!");
        beliefs.ModifyState("viewedContent", 1);
        return true;
    }

    public void AddAreaResource()
    {
        if (resource != null)
        {
            GWorld.Instance.GetQueue(locationResourceName).AddResource(resource,
                () => GWorld.Instance.GetWorld().ModifyState("FreeContentArea", 1));
        }
    }

    public void AddVisitedTarget()
    {
        _visitedLocations.Add(resource);
    }
}
