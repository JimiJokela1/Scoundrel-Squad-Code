using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Elements/LootChest", fileName = "LootChest")]
public class LootChest : Lootkeeper
{
    public int minCredits;
    public int maxCredits;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (minCredits < 0) minCredits = 0;
        if (maxCredits < 0) maxCredits = 0;
        if (maxCredits < minCredits) maxCredits = minCredits;    
    }
}
