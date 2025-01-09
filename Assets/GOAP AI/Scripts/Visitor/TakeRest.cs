using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeRest : GAction
{
    GameObject resource;
    public override bool PrePerform()
    {
        resource = GWorld.Instance.GetQueue("restAreas").RemoveResource();
        if (resource != null)
        {
            target = resource;
            GWorld.Instance.GetWorld().ModifyState("FreeRestArea", -1);
            return true;
        }

        target = null;
        Debug.LogWarning("No rest area available. Skipping rest action.");
        return false;
    }

    public override bool PostPerform()
    {
        if (resource != null)
        {
            GWorld.Instance.GetQueue("restAreas").AddResource(resource);
            GWorld.Instance.GetWorld().ModifyState("FreeRestArea", 1); // Tambahkan kembali ke queue
            beliefs.ModifyState("rested", 1);

            // Pulihkan energi agen
            Visitor visitor = GetComponent<Visitor>();
            visitor.energy = Mathf.Min(visitor.energy + 40.0f, 100.0f); // Pulihkan energi hingga maksimum 100

            // Hapus state "exhausted" jika ada
            beliefs.RemoveState("exhausted");

            Debug.Log("Rest completed. Energy restored to: " + visitor.energy);
        }
        return true;
    }
}
