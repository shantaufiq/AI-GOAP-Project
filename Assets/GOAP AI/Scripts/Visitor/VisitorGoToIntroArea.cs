using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitorGoToIntroArea : GAction
{
    GameObject resource;

    public override bool PrePerform()
    {
        GWorld.Instance.GetWorld().ModifyState("hasVisitor", -1);

        resource = inventory.FindItemWithTag("IntroArea");
        if (resource != null)
        {
            target = resource;
            return true;
        }

        beliefs.ModifyState("skipIntroArea", 1);
        beliefs.ModifyState("atIntroArea", 1);
        return true;
    }

    public override bool PostPerform()
    {
        if (resource != null)
        {
            GWorld.Instance.GetQueue("introAreas").AddResource(resource);
            GWorld.Instance.GetWorld().ModifyState("FreeIntroArea", 1);
            inventory.RemoveItem(resource);

            beliefs.ModifyState("atIntroArea", 1);
        }
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
