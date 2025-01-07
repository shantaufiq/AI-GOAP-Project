using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToVenue : GAction
{
    public override bool PrePerform()
    {
        return true;
    }

    public override bool PostPerform()
    {
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
