using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IntExtensions
{
    public static string ToStringWithSign(this int number)
    {
        if (number >= 0)
        {
            return "<color=\"green\">+" + number.ToString() + "</color>";
        }
        else
        {
            return "<color=\"red\">" + number.ToString() + "</color>";
        }
    }
}
