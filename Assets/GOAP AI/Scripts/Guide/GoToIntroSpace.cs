using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToIntroSpace : GAction
{
    public override bool PrePerform()
    {
        // Pastikan agen memiliki cukup energi sebelum menjalankan aksi
        Guide nurse = GetComponent<Guide>();
        if (nurse.energy < energyCost)
        {
            Debug.Log("Not enough energy to perform GoToCubicle");
            return false; // Gagal jika energi tidak cukup
        }

        // Cari cubicle di inventaris
        target = inventory.FindItemWithTag("Cubicle");
        if (target != null) return true;

        return false; // Gagal jika tidak ada cubicle
    }

    public override bool PostPerform()
    {
        // Modifikasi state dunia setelah aksi selesai
        GWorld.Instance.GetWorld().ModifyState("TreatingPatient", 1);

        // Tambahkan cubicle kembali ke antrian sumber daya
        GWorld.Instance.GetQueue("cubicles").AddResource(target);

        // Hapus cubicle dari inventaris agen
        inventory.RemoveItem(target);

        // Perbarui state dunia tentang cubicle yang tersedia
        GWorld.Instance.GetWorld().ModifyState("FreeCubicle", 1);

        return true;
    }
}
