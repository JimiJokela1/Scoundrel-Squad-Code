using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGlobals : MonoBehaviour
{
    public const int HIT_CHANCE_IN_COVER = 25;
    public const int HIT_CHANCE_FULL_VIEW = 75;
    public const int BASE_CRITICAL_CHANCE = 0;
    public const int ELEVATOR_ENERGY_CHARGE = 1;

    /// <summary>
    /// How many big slots can a unit have at maximum
    /// </summary>
    public const int MAX_BIG_SLOT_COUNT = 3;
    /// <summary>
    /// How many small slots can a unit have at maximum
    /// </summary>
    public const int MAX_SMALL_SLOT_COUNT = 6;
    /// <summary>
    /// How many blocks can be in unit health, armor or energy bars at maximum
    /// </summary>
    public const int MAX_UNIT_BAR_BLOCK_COUNT = 12;

    public const string CREDITS_ABBREVIATION = "KP";
}
