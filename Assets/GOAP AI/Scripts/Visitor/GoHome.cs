using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoHome : GAction
{
    private Spawner spawner;

    public override bool PrePerform()
    {
        return true;
    }

    public override bool PostPerform()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.RemoveObjectFromList(this.gameObject);
        return true;
    }
}
