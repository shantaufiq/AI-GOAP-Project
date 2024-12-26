using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetVisitor : GAction
{
    GameObject resource;

    public override bool PrePerform()
    {
        // Pastikan agen memiliki cukup energi sebelum menjalankan aksi
        Guide guide = GetComponent<Guide>();
        if (guide.energy < energyCost)
        {
            Debug.Log("Not enough energy to perform GetPatient");
            return false; // Gagal jika energi tidak cukup
        }

        // Ambil pasien dari antrian "patients"
        target = GWorld.Instance.GetQueue("patients").RemoveResource();
        if (target == null)
        {
            Debug.Log("No patients available");
            return false; // Gagal jika tidak ada pasien
        }

        // Cari cubicle kosong dari antrian "cubicles"
        resource = GWorld.Instance.GetQueue("cubicles").RemoveResource();
        if (resource != null)
        {
            inventory.AddItem(resource); // Tambahkan cubicle ke inventaris
        }
        else
        {
            // Jika tidak ada cubicle kosong, kembalikan pasien ke antrian
            GWorld.Instance.GetQueue("patients").AddResource(target);
            target = null;
            Debug.Log("No cubicles available");
            return false;
        }

        // Kurangi jumlah cubicle kosong dari state dunia
        GWorld.Instance.GetWorld().ModifyState("FreeCubicle", -1);
        return true;
    }

    public override bool PostPerform()
    {
        // Kurangi jumlah pasien yang sedang menunggu
        GWorld.Instance.GetWorld().ModifyState("Waiting", -1);

        // Tambahkan cubicle ke inventaris pasien jika tersedia
        if (target)
        {
            target.GetComponent<GAgent>().inventory.AddItem(resource);
        }

        return true;
    }
}
