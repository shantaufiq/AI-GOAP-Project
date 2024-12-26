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
            return true; // Rest area tersedia
        }
        Debug.Log("No rest area available.");
        return false; // Tidak ada rest area, aksi gagal
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
            visitor.energy = Mathf.Min(visitor.energy + 50.0f, 100.0f); // Pulihkan energi hingga maksimum 100

            // Hapus state "exhausted" jika ada
            beliefs.RemoveState("exhausted");

            Debug.Log("Rest completed. Energy restored to: " + visitor.energy);
        }
        return true;
    }
}
