using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : GAgent
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        // Menambahkan tujuan (goals) ke agen
        SubGoal s1 = new SubGoal("treatPatient", 1, false); // Tujuan merawat pasien
        goals.Add(s1, 3);

        SubGoal s2 = new SubGoal("rested", 1, false); // Tujuan beristirahat
        goals.Add(s2, 1);
    }
}
