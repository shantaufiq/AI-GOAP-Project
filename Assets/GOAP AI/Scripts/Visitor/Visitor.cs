using UnityEngine;

public class Visitor : GAgent
{
    new void Start()
    {
        base.Start();

        // Tambahkan SubGoal untuk Visitor
        SubGoal s1 = new SubGoal("atReception", 1, true);
        goals.Add(s1, 2);

        SubGoal s2 = new SubGoal("atIntroArea", 1, true);
        goals.Add(s2, 3);

        SubGoal s3 = new SubGoal("viewContent", 1, true);
        goals.Add(s3, 4);

        SubGoal s4 = new SubGoal("goingHome", 1, true);
        goals.Add(s4, 5);

        SubGoal s5 = new SubGoal("rested", 1, false);
        goals.Add(s5, 1);
    }
}
