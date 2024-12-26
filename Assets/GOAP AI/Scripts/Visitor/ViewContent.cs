using UnityEngine;

public class ViewContent : GAction
{
    public override bool PrePerform()
    {
        // Periksa apakah intro area dilewati
        if (beliefs.HasState("skipIntroArea"))
        {
            Debug.Log("Skipping directly to view content.");
            beliefs.RemoveState("skipIntroArea"); // Hapus state setelah berpindah ke konten
        }
        return true;
    }

    public override bool PostPerform()
    {
        beliefs.ModifyState("viewContent", 1);
        return true;
    }
}
