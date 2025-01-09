using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

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
    private bool isResting = false;
    private bool isSearchingMultiTarget = false;

    public NavMeshAgent myAgent;
    public Animator myAnimator;

    public void Start()
    {
        GAction[] acts = GetComponents<GAction>();
        foreach (GAction a in acts)
        {
            actions.Add(a);
        }
    }

    void Update()
    {
        if (myAgent.hasPath)
        {
            var dir = (myAgent.steeringTarget - this.transform.position).normalized;
            var animDir = this.transform.InverseTransformDirection(dir);
            var isFacingMoveDirection = Vector3.Dot(dir, transform.forward) > .5f;

            myAnimator.SetFloat("HorizontalX", isFacingMoveDirection ? animDir.x : 0, 0.5f, Time.deltaTime);
            myAnimator.SetFloat("VerticalZ", isFacingMoveDirection ? animDir.z : 0, 0.5f, Time.deltaTime);

            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(dir), 180 * Time.deltaTime);

            if (Vector3.Distance(transform.position, myAgent.destination) < myAgent.radius)
            {
                myAgent.ResetPath();
            }
        }
        else
        {
            myAnimator.SetFloat("HorizontalX", 0, 0.25f, Time.deltaTime);
            myAnimator.SetFloat("VerticalZ", 0, 0.25f, Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        if (currentAction != null && currentAction.running)
        {
            CheckCompleteAction();
            return;
        }

        PlanNewActions();
    }

    private void CheckCompleteAction()
    {
        float distanceToTarget = Vector3.Distance(destination, transform.position);

        if (currentAction is not IMultiTargetAction multiTargetAction && distanceToTarget < .9f && !invoked) // Untuk aksi biasa
        {
            currentAction.OnStartDuration();

            Invoke("CompleteAction", currentAction.duration);
            invoked = true;
        }
    }

    private void PlanNewActions()
    {
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
                    if (currentAction is IMultiTargetAction multiTargetAction)
                    {
                        StartMultiTargetCoroutine(multiTargetAction);
                    }
                    else SetActionDestination(currentAction);
                }
                else
                {
                    actionQueue = null;
                }
            }
            else
            {
                if (currentAction is IMultiTargetAction multiTargetAction)
                {
                    if (multiTargetAction.visitedLocations.Count >= multiTargetAction.targetLocations)
                    {
                        beliefs.ModifyState("viewedContent", 1);
                    }
                }

                CheckEnergy();

                GAction restAction = actions.FirstOrDefault(a => a.actionName == "TakeRest");
                if (restAction != null)
                {
                    Debug.LogWarning("Not enough energy to perform action: " + currentAction.actionName);
                    currentAction = restAction;
                }
            }
        }
    }

    private void CompleteAction()
    {
        // Debug.Log($"{this.gameObject.name} Complete Action... {currentAction.actionName}");
        currentAction.running = false;
        currentAction.OnEndDuration();
        currentAction.PostPerform();
        invoked = false;

        CheckEnergy();
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
            // Debug.LogWarning("Energy depleted! Prioritizing rest.");
            beliefs.ModifyState("exhausted", 1);
        }
    }

    private void StartMultiTargetCoroutine(IMultiTargetAction multiTargetAction)
    {
        if (!isSearchingMultiTarget)
        {
            isSearchingMultiTarget = true;
            StartCoroutine(SearchingMultiTarget(multiTargetAction));
        }
    }

    private IEnumerator SearchingMultiTarget(IMultiTargetAction multiTargetAction)
    {
        if (currentAction is not IMultiTargetAction) yield break;

        int maxAttempts = 50;
        int attempt = 0;

        while (multiTargetAction.visitedLocations.Count < multiTargetAction.targetLocations && attempt < maxAttempts)
        {
            attempt++;
            // Debug.Log($"Starting multi-target action. Attempt {attempt}/{maxAttempts}");

            currentAction.target = multiTargetAction.GetNextTarget();

            if (currentAction.target != null)
            {
                // Debug.Log($"Target acquired: {currentAction.target.name}");
                SetActionDestination(currentAction);
                yield return new WaitUntil(() => !currentAction.agent.pathPending && currentAction.agent.remainingDistance < .9f);
                currentAction.OnStartDuration();
                yield return new WaitForSeconds(currentAction.duration);
                currentAction.OnEndDuration();

                multiTargetAction.AddAreaResource();
                multiTargetAction.AddVisitedTarget();

                if (energy <= 0 && !isResting)
                {
                    isResting = true;
                    GAction restAction = actions.FirstOrDefault(a => a.actionName == "TakeRest");

                    if (restAction != null)
                    {
                        currentAction = restAction;

                        if (currentAction.PrePerform())
                        {
                            SetActionDestination(currentAction);
                            yield return StartCoroutine(ExecuteRestAction(currentAction));// Tunggu hingga istirahat selesai
                        }
                    }

                    isResting = false; // Reset flag setelah istirahat
                }
            }
            else
            {
                Debug.LogWarning("No more valid targets for multi-target action.");
                break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        if (multiTargetAction.visitedLocations.Count >= multiTargetAction.targetLocations)
        {
            Debug.Log("Multi-target action completed.");
            CompleteAction();
        }
        else
        {
            Debug.LogError("Failed to complete multi-target action within allowed attempts.");
        }

        isSearchingMultiTarget = false;
    }

    private IEnumerator ExecuteRestAction(GAction savedAction)
    {
        while (Vector3.Distance(destination, transform.position) > 2f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(currentAction.duration + 1f);

        isResting = false;

        // Pastikan currentAction diatur kembali ke aksi yang disimpan
        currentAction = savedAction;

        // Jangan panggil SetActionDestination lagi jika sudah dilakukan di luar
        if (!currentAction.running && currentAction.PrePerform())
        {
            SetActionDestination(currentAction);
        }
    }
}
