using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceQueue
{
    public Queue<GameObject> que = new Queue<GameObject>();
    public string tag;
    public string modState;

    public ResourceQueue(string t, string ms, WorldStates w)
    {
        tag = t;
        modState = ms;
        if (tag != "")
        {
            GameObject[] resources = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject r in resources)
                que.Enqueue(r);
        }

        if (modState != "")
        {
            w.ModifyState(modState, que.Count);
        }
    }

    public void AddResource(GameObject r, Action endAction = null)
    {
        if (!que.Contains(r))
        {
            que.Enqueue(r);
            if (endAction != null) endAction.Invoke();
        }
        else
        {
            Debug.Log("Resource sudah ada dalam antrian: " + r.name);
        }
    }

    public GameObject RemoveResource()
    {
        if (que.Count == 0) return null;
        return que.Dequeue();
    }

    public GameObject RemoveResource(GameObject target)
    {
        if (que.Contains(target))
        {
            var tempQueue = new Queue<GameObject>();
            while (que.Count > 0)
            {
                GameObject resource = que.Dequeue();
                if (resource != target)
                {
                    tempQueue.Enqueue(resource);
                }
            }
            que = tempQueue;
            return target;
        }
        return null;
    }
}

public sealed class GWorld
{
    private static readonly GWorld instance = new GWorld();
    private static WorldStates world;
    private static ResourceQueue patients;
    private static ResourceQueue cubicles;

    private static ResourceQueue visitors;
    private static ResourceQueue introArea;
    private static ResourceQueue restArea;
    private static ResourceQueue contentArea;
    private static Dictionary<string, ResourceQueue> resources = new Dictionary<string, ResourceQueue>();

    static GWorld()
    {
        world = new WorldStates();
        patients = new ResourceQueue("", "", world);
        resources.Add("patients", patients);
        cubicles = new ResourceQueue("Cubicle", "FreeCubicle", world);
        resources.Add("cubicles", cubicles);

        visitors = new ResourceQueue("", "", world);
        resources.Add("visitors", visitors);
        introArea = new ResourceQueue("IntroArea", "FreeIntroArea", world);
        resources.Add("introAreas", introArea);
        restArea = new ResourceQueue("RestArea", "FreeRestArea", world);
        resources.Add("restAreas", restArea);
        contentArea = new ResourceQueue("ContentArea", "FreeContentArea", world);
        resources.Add("contentArea", contentArea);

        Time.timeScale = 5;
    }

    public ResourceQueue GetQueue(string type)
    {
        if (!resources.ContainsKey(type))
        {
            Debug.LogError($"Queue '{type}' not found in GWorld.");
            return null;
        }
        return resources[type];
    }

    public void AddQueue(string type, ResourceQueue queue)
    {
        if (!resources.ContainsKey(type))
        {
            resources.Add(type, queue);
        }
    }

    private GWorld()
    {
    }

    public static GWorld Instance
    {
        get { return instance; }
    }

    public WorldStates GetWorld()
    {
        return world;
    }
}
