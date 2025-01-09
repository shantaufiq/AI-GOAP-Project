using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceptionistRegister : GAction
{
    GameObject npcGObject;
    GameObject resource;
    public override bool PrePerform()
    {
        npcGObject = GWorld.Instance.GetQueue("visitors").RemoveResource();
        if (npcGObject == null)
            return false;

        GameObject temp = GWorld.Instance.GetQueue("introAreas").RemoveResource();
        resource = temp ? temp : null;

        GWorld.Instance.GetWorld().ModifyState("FreeIntroArea", -1);

        return true;
    }

    public override bool PostPerform()
    {
        if (npcGObject && resource != null)
        {
            Debug.Log($"{npcGObject?.name} Should walk INTRO AREA: ${resource?.name}");
            npcGObject.GetComponent<GAgent>().inventory.AddItem(resource);
        }

        return true;
    }
}
