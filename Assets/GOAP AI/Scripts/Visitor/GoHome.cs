using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoHome : GAction
{
    public override bool PrePerform()
    {
        return true;
    }

    public override bool PostPerform()
    {
        // Debug.Log("NPC going home.");
        Destroy(this.gameObject, 1.0f);
        return true;
    }
}
