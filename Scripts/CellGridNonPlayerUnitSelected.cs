using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGridNonPlayerUnitSelected : CellGridState
{
    public override bool IsPlayerTurn()
    {
        return true;
    }
    
    public CellGridNonPlayerUnitSelected(CellGrid cellGrid) : base(cellGrid)
    {
    }
}
