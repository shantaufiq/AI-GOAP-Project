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

    private GPlanner planner;
    private Queue<GAction> actionQueue;
    public GAction currentAction;
    private SubGoal currentGoal;
    private Vector3 destination = Vector3.zero;

    private bool invoked = false;
    private bool isStaying = false;

    public void Start()
    {
        GAction[] acts = GetComponents<GAction>();
        foreach (GAction a in acts)
        {
            actions.Add(a);
        }
    }

    private void LateUpdate()
    {
        ManageVisitorBehavior();

        if (currentAction != null && currentAction.running)
        {
            HandleCurrentAction();
            return;
        }

        PlanNewActions();
    }

    private void HandleCurrentAction()
    {
        float distanceToTarget = Vector3.Distance(destination, transform.position);

        if (currentAction is IMultiTargetAction multiTargetAction)
        {
            if (multiTargetAction.visitedLocations.Count < multiTargetAction.targetLocations)
            {
                if (distanceToTarget < 2f && !isStaying)
                {
                    isStaying = true;
                    StartCoroutine(StayAtLocation(multiTargetAction));
                    return;
                }
            }
            else
            {
                // Semua target sudah dikunjungi
                currentAction.running = false;
                Debug.Log("All multi-target locations visited.");
            }
        }
        else if (distanceToTarget < 2f && !invoked) // Untuk aksi biasa
        {
            Invoke("CompleteAction", currentAction.duration);
            invoked = true;
        }
    }

    private IEnumerator StayAtLocation(IMultiTargetAction multiTargetAction)
    {
        Debug.Log("Staying at location for duration: " + currentAction.duration);
        yield return new WaitForSeconds(currentAction.duration);

        if (multiTargetAction.visitedLocations.Count < multiTargetAction.targetLocations)
        {
            SetNextMultiTarget(multiTargetAction);
        }
        else
        {
            Debug.Log("All target locations visited.");
            currentAction.running = false; // Pastikan aksi dihentikan
        }

        isStaying = false;
    }

    private void PlanNewActions()
    {
        if (planner == null || actionQueue == null)
        {
            planner = new GPlanner();
            var sortedGoals = goals.OrderByDescending(g => g.Value);

            foreach (var sg in sortedGoals)
            {
                if ((energy <= 0 && sg.Key.sgoals.ContainsKey("rested")) || energy > 0)
                {
                    actionQueue = planner.plan(actions, sg.Key.sgoals, beliefs);
                    if (actionQueue != null)
                    {
                        currentGoal = sg.Key;
                        Debug.Log($"Selected goal: {sg.Key.sgoals.Keys.First()} with priority {sg.Value}");
                        break;
                    }
                }
            }
        }

        if (actionQueue != null && actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();

            if (currentAction.IsAchievableWithEnergy(energy))
            {
                if (currentAction.PrePerform())
                {
                    SetActionDestination(currentAction);
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
        else if (actionQueue != null && actionQueue.Count == 0)
        {
            if (currentGoal.remove)
            {
                goals.Remove(currentGoal);
            }
            planner = null;
        }
    }

    private void CompleteAction()
    {
        Debug.Log($"Complete Action... {currentAction.actionName}");
        currentAction.running = false;
        currentAction.PostPerform();
        invoked = false;
    }

    private void SetActionDestination(GAction action)
    {
        if (action.target == null && action.targetTag != "")
        {
            action.target = GameObject.FindWithTag(action.targetTag);
        }

        if (action.target != null)
        {
            Transform dest = action.target.transform.Find("Destination");
            destination = dest != null ? dest.position : action.target.transform.position;
            action.agent.SetDestination(destination);

            action.running = true;
            energy -= action.energyCost;
            CheckEnergy();
        }
        else
        {
            Debug.LogWarning("Action target not found: " + action.actionName);
            actionQueue = null;
        }
    }

    private void CheckEnergy()
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

    private void ManageVisitorBehavior()
    {
        if (beliefs.HasState("exhausted") && energy >= 80)
        {
            beliefs.RemoveState("exhausted");
            Debug.Log("Visitor is rested, resuming normal behavior.");
        }
    }

    private void SetNextMultiTarget(IMultiTargetAction multiTargetAction)
    {
        multiTargetAction.AddAreaResource();

        if (multiTargetAction.visitedLocations.Count >= multiTargetAction.targetLocations)
        {
            Debug.Log("All multi-target locations visited. Ending action.");
            currentAction.running = false; // Hentikan aksi
            return;
        }

        currentAction.target = multiTargetAction.GetNextTarget();
        if (currentAction.target != null)
        {
            SetActionDestination(currentAction);
        }
        else
        {
            Debug.LogError("No next target found for multi-target action.");
        }
    }
}
