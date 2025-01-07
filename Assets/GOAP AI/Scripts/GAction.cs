using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class GAction : MonoBehaviour
{
    public string actionName = "Action";
    public float cost = 1.0f;
    public float energyCost = 10.0f; // Energi yang dibutuhkan untuk menjalankan aksi
    public GameObject target;
    public string targetTag;
    public float duration = 0;
    public bool hasMultiLocationTarget = false;
    public string locationResourceName = "";
    public WorldState[] preConditions;
    public WorldState[] afterEffects;
    public NavMeshAgent agent;

    public Dictionary<string, int> preconditions;
    public Dictionary<string, int> effects;

    public GInventory inventory;
    public WorldStates beliefs;
    public Animator myAnimator;

    public bool running = false;

    public GAction()
    {
        preconditions = new Dictionary<string, int>();
        effects = new Dictionary<string, int>();
    }

    public void Awake()
    {
        agent = this.gameObject.GetComponent<NavMeshAgent>();

        if (preConditions != null)
            foreach (WorldState w in preConditions)
            {
                preconditions.Add(w.key, w.value);
            }

        if (afterEffects != null)
            foreach (WorldState w in afterEffects)
            {
                effects.Add(w.key, w.value);
            }

        inventory = this.GetComponent<GAgent>().inventory;
        beliefs = this.GetComponent<GAgent>().beliefs;
        myAnimator = this.GetComponent<GAgent>().myAnimator;
    }

    public bool IsAchievable()
    {
        return true;
    }

    public bool IsAchievableGiven(Dictionary<string, int> conditions)
    {
        foreach (KeyValuePair<string, int> p in preconditions)
        {
            if (!conditions.ContainsKey(p.Key))
                return false;
        }
        return true;
    }

    // Tambahkan logika untuk memeriksa energi
    public bool IsAchievableWithEnergy(float currentEnergy)
    {
        return currentEnergy >= energyCost;
    }

    public abstract bool PrePerform();
    public abstract bool PostPerform();
    public virtual void OnStartDuration() { }
    public virtual void OnEndDuration() { }
}

public interface IMultiTargetAction
{
    bool hasMultipleTargets { get; }
    int targetLocations { get; }
    List<GameObject> visitedLocations { get; }
    GameObject GetNextTarget();
    void AddAreaResource();
    void AddVisitedTarget();
}