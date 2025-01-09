using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToReception : GAction
{
    public override bool PrePerform()
    {
        GWorld.Instance.GetQueue("visitors").AddResource(this.gameObject);
        GWorld.Instance.GetWorld().ModifyState("hasVisitor", 1);
        return true;
    }

    public override bool PostPerform()
    {
        beliefs.ModifyState("atReception", 1);

        return true;
    }

    public override void OnStartDuration()
    {
        base.OnStartDuration();
        myAnimator.SetTrigger("talkingTrigger");
    }

    public override void OnEndDuration()
    {
        base.OnEndDuration();
        myAnimator.SetTrigger("idleTriggerr");
    }
}
