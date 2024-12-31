using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SubGoal
{
    public Dictionary<string, int> sgoals;
    public bool remove;

    public SubGoal(string s, int i, bool r)
    {
        sgoals = new Dictionary<string, int>();
        sgoals.Add(s, i);
        remove = r;
    }
}

public abstract class GAgent : MonoBehaviour
{
    public float energy = 100.0f; // Energi NPC saat ini
    public List<GAction> actions = new List<GAction>();
    public Dictionary<SubGoal, int> goals = new Dictionary<SubGoal, int>();
    public GInventory inventory = new GInventory();
    public WorldStates beliefs = new WorldStates();

    GPlanner planner;
    Queue<GAction> actionQueue;
    public GAction currentAction;
    SubGoal currentGoal;

    Vector3 destination = Vector3.zero;

    public void Start()
    {
        GAction[] acts = this.GetComponents<GAction>();
        foreach (GAction a in acts)
            actions.Add(a);
    }

    bool invoked = false;

    void CompleteAction()
    {
        if (currentAction is IMultiTargetAction multiTargetAction)
        {
            if (multiTargetAction.visitedLocations.Count < multiTargetAction.targetLocations)
            {
                Debug.Log($"Action {currentAction.actionName} still has targets to visit.");
                currentAction.target = multiTargetAction.GetNextTarget();

                if (currentAction.target != null)
                {
                    Transform dest = currentAction.target.transform.Find("Destination");
                    destination = dest != null ? dest.position : currentAction.target.transform.position;
                    currentAction.agent.SetDestination(destination);

                    // Pengurangan energi untuk aksi baru
                    energy -= currentAction.energyCost;
                    CheckEnergy();

                    multiTargetAction.AddAreaResource();

                    currentAction.running = true;
                    invoked = false;
                }
                return;
            }
        }

        // Jika semua target selesai
        currentAction.running = false;
        currentAction.PostPerform();
        invoked = false;
    }

    void LateUpdate()
    {
        if (currentAction != null && currentAction.running)
        {
            float distanceToTarget = Vector3.Distance(destination, this.transform.position);

            if (currentAction is IMultiTargetAction multiTargetAction)
            {
                if (multiTargetAction.visitedLocations.Count < multiTargetAction.targetLocations)
                {
                    if (distanceToTarget < 2f)
                    {
                        Debug.Log($"Still visiting targets: {multiTargetAction.visitedLocations.Count}/{multiTargetAction.targetLocations}");

                        // Ambil target berikutnya
                        currentAction.target = multiTargetAction.GetNextTarget();
                        if (currentAction.target != null)
                        {
                            Transform dest = currentAction.target.transform.Find("Destination");
                            destination = dest != null ? dest.position : currentAction.target.transform.position;
                            currentAction.agent.SetDestination(destination); // Tetapkan tujuan berikutnya

                            currentAction.running = true; // Tetap jalankan aksi
                            invoked = false; // Reset invoked untuk perjalanan berikutnya
                        }
                        else
                        {
                            Debug.LogError("No next target found.");
                            currentAction.running = false; // Hentikan aksi jika tidak ada target lagi
                        }

                        multiTargetAction.AddAreaResource();
                    }
                    return; // Hentikan sementara hingga tujuan berikutnya diproses
                }
            }

            // Proses penyelesaian aksi jika tidak ada target tersisa
            if (distanceToTarget < 2f)
            {
                if (!invoked)
                {
                    Invoke("CompleteAction", currentAction.duration);
                    invoked = true;
                }
            }
            return;
        }

        // Perencanaan aksi baru jika tidak ada aksi yang sedang berjalan
        if (planner == null || actionQueue == null)
        {
            planner = new GPlanner();

            var sortedGoals = from entry in goals orderby entry.Value descending select entry;

            foreach (KeyValuePair<SubGoal, int> sg in sortedGoals)
            {
                if (energy <= 0 && sg.Key.sgoals.ContainsKey("rested"))
                {
                    // Prioritaskan goal "rested" jika energi habis
                    actionQueue = planner.plan(actions, sg.Key.sgoals, beliefs);
                    if (actionQueue != null)
                    {
                        currentGoal = sg.Key;
                        break;
                    }
                }
                else if (energy > 0)
                {
                    // Jalankan goal lain jika energi mencukupi
                    actionQueue = planner.plan(actions, sg.Key.sgoals, beliefs);
                    if (actionQueue != null)
                    {
                        currentGoal = sg.Key;
                        break;
                    }
                }
            }
        }

        if (actionQueue != null && actionQueue.Count == 0)
        {
            if (currentGoal.remove)
            {
                goals.Remove(currentGoal);
            }
            planner = null;
        }

        if (actionQueue != null && actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();

            if (currentAction.IsAchievableWithEnergy(energy))
            {
                if (currentAction.PrePerform())
                {
                    if (currentAction.target == null && currentAction.targetTag != "")
                        currentAction.target = GameObject.FindWithTag(currentAction.targetTag);

                    if (currentAction.target != null)
                    {
                        currentAction.running = true;

                        // Kurangi energi saat memulai aksi baru
                        energy -= currentAction.energyCost;
                        CheckEnergy();

                        Transform dest = currentAction.target.transform.Find("Destination");
                        destination = dest != null ? dest.position : currentAction.target.transform.position;
                        currentAction.agent.SetDestination(destination);

                        // Handle actions with multiple targets
                        if (currentAction is IMultiTargetAction multiTargetAction &&
                            multiTargetAction.visitedLocations.Count < multiTargetAction.targetLocations)
                        {
                            Debug.Log($"Starting multi-target action: {currentAction.actionName}");
                            return;
                        }
                    }
                }
                else
                {
                    actionQueue = null;
                }
            }
            else
            {
                Debug.LogWarning("Not enough energy to perform action: " + currentAction.actionName);
                actionQueue = null;
            }
        }
    }

    // Fungsi untuk memeriksa energi
    void CheckEnergy()
    {
        if (energy <= 0)
        {
            Debug.LogWarning("Energy depleted! Prioritizing rest.");
            beliefs.ModifyState("exhausted", 1);
        }
        else
        {
            beliefs.RemoveState("exhausted");
        }
    }
}
