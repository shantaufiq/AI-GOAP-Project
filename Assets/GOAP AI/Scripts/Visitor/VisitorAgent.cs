using UnityEngine;

public class VisitorAgent : GAgent
{
    new void Start()
    {
        base.Start();

        // Menambahkan tujuan untuk registrasi di resepsionis
        SubGoal s1 = new SubGoal("isRegisted", 1, true);
        goals.Add(s1, 1);
    }
}
