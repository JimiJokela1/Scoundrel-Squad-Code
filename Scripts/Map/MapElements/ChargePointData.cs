using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Elements/Charge Point", fileName = "ChargePoint")]
public class ChargePointData : ScriptableObject
{
    public string pointName;
    public string description;

    public ChargePoint.ChargeType chargeType;
    public int minPrice;
    public int maxPrice;

    public int minCharges;
    public int maxCharges;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(pointName)) pointName = name;
        if (description == null) description = "";
        
        if (minPrice < 0) minPrice = 0;
        if (maxPrice < 0) maxPrice = 0;
        if (maxPrice < minPrice) maxPrice = minPrice;

        if (minCharges < 0) minCharges = 0;
        if (maxCharges < 0) maxCharges = 0;
        if (maxCharges < minCharges) maxCharges = minCharges;
    }
}
