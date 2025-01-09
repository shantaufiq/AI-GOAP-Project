using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceptionistStandby : GAction
{
    public override bool PrePerform()
    {

        GWorld.Instance.GetWorld().ModifyState("hasVisitor", 0);
        return true;
    }

    public override bool PostPerform()
    {
        return true;
    }
}
