using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer : Player
{
    [Tooltip("Players that this AI considers enemies")]
    public List<int> enemyPlayerNumbers;

    public int minPatrolLength;
    public int maxPatrolLength;

    private CellGrid _cellGrid;
    private Dictionary<UberUnit, AIUnit> _aiUnits;

    private bool initializedUnits = false;

    private bool playing = false;
    private bool myTurn = false;

    public AIUnit GetAIUnit(UberUnit unit)
    {
        if (_aiUnits != null && _aiUnits.ContainsKey(unit))
        {
            return _aiUnits[unit];
        }

        return null;
    }

    public AIPlayer()
    {
        _aiUnits = new Dictionary<UberUnit, AIUnit>();
        initializedUnits = false;
    }

    public override void OnLevelStarted()
    {
        if (_cellGrid == null)
        {
            _cellGrid = GameMap.Instance.cellGrid;
        }
        initializedUnits = false;
        InitializeUnits();
    }

    private void InitializeUnits()
    {
        if (initializedUnits)
        {
            return;
        }
        
        _aiUnits = new Dictionary<UberUnit, AIUnit>();
        List<Unit> myUnits = UnitManager.Instance.allUnits.FindAll(u => u.PlayerNumber.Equals(PlayerNumber)).ToList();

        foreach (Unit myUnit in myUnits)
        {
            if (!(myUnit is UberUnit))
            {
                Debug.LogError("Wrong Unit type");
                continue;
            }

            UberUnit myUberUnit = (UberUnit) myUnit;

            if (!_aiUnits.ContainsKey(myUberUnit))
            {
                _aiUnits.Add(myUberUnit, new AIUnit(myUberUnit, this, _cellGrid, false, minPatrolLength, maxPatrolLength));
            }
        }

        foreach (UberUnit unit in _aiUnits.Keys)
        {
            unit.UnitDestroyed += OnUnitDestroyed;
            unit.UnitAttacked += OnUnitAttacked;
        }

        initializedUnits = true;
    }

    private void OnUnitAttacked(object sender, AttackEventArgs e)
    {
        if (!(sender is UberUnit) || !(e.Attacker is UberUnit))
        {
            Debug.LogError("wrong Unit type");
            return;
        }

        if (_aiUnits.ContainsKey((UberUnit) sender))
        {
            _aiUnits[(UberUnit) sender].UnitAttacked((UberUnit) e.Attacker);
        }
    }

    private void OnUnitDestroyed(object sender, AttackEventArgs e)
    {
        if (!(sender is UberUnit))
        {
            Debug.LogError("wrong Unit type");
            return;
        }

        UberUnit unit = (UberUnit) sender;

        if (_aiUnits.ContainsKey(unit))
        {
            _aiUnits[unit].UnitDied();
            _aiUnits.Remove(unit);
        }

    }

    public override void Play(CellGrid cellGrid)
    {
        cellGrid.CellGridState = new CellGridStateAiTurn(cellGrid);
        _cellGrid = cellGrid;

        if (!initializedUnits)
        {
            InitializeUnits();
        }

        myTurn = true;
        playing = true;

        StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        // Moved actual end turn call to update so it's not inside the coroutine, 
        // because debugging was more difficult and buggy when error happens in coroutine
        if (myTurn)
        {
            if(!playing)
            {
                _cellGrid.EndTurn();
                myTurn = false;
            }
        }
    }

    private IEnumerator PlaySequence()
    {
        List<UberUnit> myUnits = _aiUnits.Keys.ToList();
        foreach (var unit in myUnits.OrderByDescending(u => Random.value))
        {
            if (!unit.IsAlive())
            {
                continue;
            }
            List<Unit> enemyUnits = new List<Unit>();
            if (enemyPlayerNumbers != null && enemyPlayerNumbers.Count > 0)
            {
                enemyUnits = UnitManager.Instance.allUnits.FindAll(u => enemyPlayerNumbers.Contains(u.PlayerNumber)).ToList();
            }

            List<UberUnit> enemies = new List<UberUnit>();
            foreach (var enemyUnit in enemyUnits)
            {
                if (enemyUnit is UberUnit)
                {
                    enemies.Add((UberUnit) enemyUnit);
                }
            }

            yield return StartCoroutine(_aiUnits[unit].PlaySequence(enemies));
        }

        playing = false;
    }
}
