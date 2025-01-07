using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitorGoToIntroArea : GAction
{
    GameObject resource;

    public override bool PrePerform()
    {
        resource = GWorld.Instance.GetQueue("introAreas").RemoveResource();
        if (resource != null)
        {
            target = resource;
            GWorld.Instance.GetWorld().ModifyState("FreeIntroArea", -1);
            return true; // Area intro tersedia
        }
        Debug.Log("No intro area available. Skipping to view content.");
        beliefs.ModifyState("skipIntroArea", 1); // Tandai intro area dilewati
        beliefs.ModifyState("atIntroArea", 1); // Tandai intro area dilewati
        return true; // Tetap lanjutkan ke aksi berikutnya
    }

    public override bool PostPerform()
    {
        if (resource != null)
        {
            GWorld.Instance.GetQueue("introAreas").AddResource(resource);
            GWorld.Instance.GetWorld().ModifyState("FreeIntroArea", 1);
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
