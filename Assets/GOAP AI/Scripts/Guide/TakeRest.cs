using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeRest : GAction
{
    public override bool PrePerform()
    {
        // Tidak ada precondition khusus untuk aksi Rest
        return true;
    }

    public override bool PostPerform()
    {
        // Pulihkan energi agen
        Visitor visitor = GetComponent<Visitor>();
        visitor.energy = Mathf.Min(visitor.energy + 50.0f, 100.0f); // Pulihkan energi hingga maksimum 100

        // Hapus state "exhausted" dari beliefs
        beliefs.RemoveState("exhausted");

        Debug.Log("Rest completed. Energy restored to: " + visitor.energy);

        return true;
    }
}
