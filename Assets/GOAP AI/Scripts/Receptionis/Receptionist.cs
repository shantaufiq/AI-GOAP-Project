using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Receptionist : GAgent
{
    new void Start()
    {
        base.Start();

        SubGoal s1 = new SubGoal("servingVisitor", 1, false);
        goals.Add(s1, 3);

        SubGoal s2 = new SubGoal("isStandby", 1, false);
        goals.Add(s2, 2);

        SubGoal s3 = new SubGoal("rested", 1, false);
        goals.Add(s3, 1);

        // Invoke("GetTired", Random.Range(10f, 20f));

        GWorld.Instance.GetWorld().ModifyState("waitingVisitor", 0);
    }

    void GetTired()
    {
        beliefs.ModifyState("exhausted", 0);
        Invoke("GetTired", Random.Range(10f, 20f));
    }
}
