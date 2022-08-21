using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base class for player and enemy units in Scoundrel Squad
/// </summary>
public abstract class UberUnit : Unit
{
    Dictionary<Cell, List<Cell>> cachedAttackPaths = null;

    private event EventHandler UnitTurnStarted;

    public List<Equipment> equipped
    {
        get { return core.equipped; }
    }
    public Equipment activeEquipment;
    public UnitCore core;

    public int TotalEnergy { get; protected set; }

    [HideInInspector]
    public int currentEnergy;

    protected Dictionary<Cell, List<Cell>> carelessCachedPaths;

    public int TotalArmor
    {
        get
        {
            int armor = 0;
            foreach (Equipment equipment in equipped)
            {
                armor += equipment.data.bonusArmor;
            }
            return armor;
        }
        protected set
        {

        }
    }

    public int currentArmor
    {
        get
        {
            int armor = 0;
            foreach (Equipment equipment in equipped)
            {
                armor += equipment.armorPoints;
            }
            return armor;
        }
        set
        {
            int mod = value - currentArmor;
            foreach (Equipment equipment in equipped)
            {
                if (mod > 0)
                {
                    int equipmentArmorDeficit = equipment.data.bonusArmor - equipment.armorPoints;
                    if (equipmentArmorDeficit > 0)
                    {
                        if (mod > equipmentArmorDeficit)
                        {
                            equipment.armorPoints = equipment.data.bonusArmor;
                            mod -= equipmentArmorDeficit;
                        }
                        else
                        {
                            equipment.armorPoints += mod;
                            mod = 0;
                        }
                    }
                }
                else if (mod < 0)
                {
                    if (equipment.armorPoints > 0)
                    {
                        if (-mod > equipment.armorPoints)
                        {
                            mod += equipment.armorPoints;
                            equipment.armorPoints = 0;
                        }
                        else
                        {
                            equipment.armorPoints += mod;
                            mod = 0;
                        }
                    }
                }
            }
        }
    }

    [Tooltip("Speed at which model rotates when moving.")]
    public float rotateSpeed;
    [Tooltip("How many degrees is the model's forward direction rotated to the left.")]
    public float modelForwardRotation;

    [Tooltip("For player units, used only to get the model prefab. For enemy units, defines also the random death loot.")]
    public LootChest deathLoot;

    [Tooltip("Point where gun shot effects will appear")]
    public Transform gunBarrelPoint;
    [Tooltip("Point where effect will appear when this unit is shot etc. Unit chest area for example.")]
    public Transform targetPoint;

    public UnitWorldUI unitWorldUI;

    public event EventHandler UnitUsedAction;

    private bool initialized = false;

    public override string ToDescription()
    {
        string desc = "";

        if (Equipment.IsEmptyEquipment(activeEquipment))
        {
            desc += "Equipped: None\n";
        }
        else
        {
            desc += "Equipped: " + activeEquipment.GetItemName() + "\n";
        }

        desc += base.ToDescription();
        return desc;
    }

    public bool HasActionsOrMovementLeft()
    {
        return ActionPoints > 0 || MovementPoints > 0;
    }

    public bool HasMovementLeft()
    {
        return MovementPoints > 0;
    }

    public override void Initialize()
    {
        if (initialized) return;
        initialized = true;

        Buffs = new List<Buff>();

        UnitState = new UnitStateNormal(this);

        if (GetComponent<UnitStartingEquipment>() != null)
        {
            GetComponent<UnitStartingEquipment>().ApplyStartingEquipment();
        }

        if (equipped != null && equipped.Count != 0)
        {
            activeEquipment = equipped[0];
        }
        else
        {
            activeEquipment = Equipment.EmptyEquipment();
        }

        core.Initialize(this);
        HitPoints = TotalHitPoints;
        currentArmor = TotalArmor;
        currentEnergy = TotalEnergy;
        MovementPoints = TotalMovementPoints;
        ActionPoints = TotalActionPoints;

        if (unitWorldUI != null)
        {
            unitWorldUI.Init(GameGlobals.MAX_UNIT_BAR_BLOCK_COUNT, this);
        }
    }

    public void ChangeCore(UnitCoreData newCore)
    {
        List<Equipment> unEquippedList = null;
        if (core != null && core.data != null)
        {
            unEquippedList = new List<Equipment>(equipped);
            foreach (Equipment equipment in unEquippedList)
            {
                UnequipItem(equipment);
            }

        }

        core = new UnitCore(newCore);
        core.Initialize(this);

        if (unEquippedList != null)
        {
            List<Equipment> cantEquip = new List<Equipment>();
            foreach (Equipment unEquipped in unEquippedList)
            {
                if (!EquipItem(unEquipped))
                {
                    cantEquip.Add(unEquipped);
                }
            }

            // place loot drop on tile
            if (cantEquip.Count > 0)
            {
                foreach (Equipment equipment in cantEquip)
                {
                    LootSpawner.Instance.DropItem(Cell, equipment);
                }
            }
        }
    }

    private void OnUnitUsedAction()
    {
        if (UnitUsedAction != null)
        {
            UnitUsedAction.Invoke(this, new EventArgs());
        }
    }

    public bool OnEquipmentSelected(Equipment equipment)
    {
        if (!equipped.Contains(equipment))
        {
            Debug.LogError("Trying to choose active an equipment that is not equipped");
            return false;
        }

        if (equipment.data.activationType == EquipmentActivationType.Passive)
        {
            return false;
        }

        // Change active equipment
        activeEquipment = equipment;

        return true;
    }

    public bool TryReload(Equipment equipment)
    {
        // Check if reload needed and try to reload
        if (equipment is Weapon)
        {
            Weapon weapon = (Weapon) equipment;
            if (weapon.loadedAmmo == 0 && weapon.weaponData.rangeType != WeaponRangeType.Melee)
            {
                if (ActionPoints > 0 && ReloadWeapon(weapon))
                {
                    ActionPoints--;
                    return true;
                }
            }
        }
        return false;
    }

    public int GetHitChance(UberUnit targetUnit)
    {
        return activeEquipment.GetHitChance(this, targetUnit);
    }

    public string GetTargetingDescription(UberUnit targetUnit)
    {
        return activeEquipment.GetTargetingDescription(this, targetUnit);
    }

    public virtual bool ReloadWeapon(Weapon weapon)
    {
        int reloadAmount = 0;
        int availableAmmo = 0;
        List<AmmoStack> ammoStacks = new List<AmmoStack>();
        foreach (Equipment equipment in equipped)
        {
            if (equipment is AmmoStack)
            {
                AmmoStack ammoStack = (AmmoStack) equipment;
                if (ammoStack.data == weapon.weaponData.ammoType)
                {
                    availableAmmo += ammoStack.ammoCount;
                    ammoStacks.Add(ammoStack);
                }
            }
        }

        if (availableAmmo >= weapon.weaponData.ammoCapacity)
        {
            reloadAmount = weapon.weaponData.ammoCapacity;
        }
        else
        {
            if (weapon.weaponData.burstCount == 0)
            {
                Debug.LogError("Weapon should not have a burst count of 0.");
            }
            else
            {
                reloadAmount = availableAmmo - (availableAmmo % weapon.weaponData.burstCount);
            }
        }

        if (reloadAmount == 0)
        {
            return false;
        }

        // Remove reloadAmount of ammo from ammostacks
        int removedAmmo = 0;
        foreach (AmmoStack ammoStack in ammoStacks)
        {
            if (ammoStack.ammoCount >= (reloadAmount - removedAmmo))
            {
                ammoStack.ammoCount -= (reloadAmount - removedAmmo);
                removedAmmo = reloadAmount;
            }
            else
            {
                removedAmmo += ammoStack.ammoCount;
                ammoStack.ammoCount = 0;
            }

            if (ammoStack.ammoCount == 0)
            {
                UnequipItem(ammoStack);
            }

            if (removedAmmo == reloadAmount)
            {
                break;
            }
        }

        weapon.loadedAmmo = reloadAmount;
        return true;
    }

    public override string GetUnitName()
    {
        return name;
    }

    public override void OnTurnEnd()
    {
        base.OnTurnEnd();

        cachedAttackPaths = null;
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();

        if (UnitTurnStarted != null)
        {
            UnitTurnStarted.Invoke(this, new EventArgs());
        }
    }

    public virtual void Heal(int healAmount)
    {
        HitPoints += healAmount;
        HitPoints = (HitPoints > TotalHitPoints) ? TotalHitPoints : HitPoints;

        if (unitWorldUI != null)
        {
            unitWorldUI.UpdateBars(this);
        }
    }

    public virtual void RechargeArmor(int rechargeAmount)
    {
        currentArmor += rechargeAmount;
        currentArmor = (currentArmor > TotalArmor) ? TotalArmor : currentArmor;

        if (unitWorldUI != null)
        {
            unitWorldUI.UpdateBars(this);
        }
    }

    public virtual void RechargeEnergy(int rechargeAmount)
    {
        currentEnergy += rechargeAmount;
        currentEnergy = (currentEnergy > TotalEnergy) ? TotalEnergy : currentEnergy;

        if (unitWorldUI != null)
        {
            unitWorldUI.UpdateBars(this);
        }
    }

    public override void DealDamage(Unit defender, WeaponAttack attack)
    {
        if (!(defender is UberUnit))
        {
            Debug.LogError("Wrong unit type to use this method.");
            return;
        }

        UberUnit defenderUberUnit = (UberUnit) defender;
        defenderUberUnit.Defend(this, attack);
    }

    protected override void Defend(Unit attacker, WeaponAttack attack)
    {
        if (!(attacker is UberUnit))
        {
            Debug.LogError("Wrong unit type to use this method");
            return;
        }

        UberUnit attackerUberUnit = (UberUnit) attacker;

        MarkAsDefending(attackerUberUnit);

        // For logging
        int actualArmorDamage = 0;
        // For logging
        int actualHealthDamage = 0;

        // Do damage based on damage type
        switch (attack.damageType)
        {
            // Light does half damage to armor first (2 points of damage to destroy 1 point of armor), then leftovers to health
            case DamageType.Light:
                int armorDamage = Mathf.FloorToInt(attack.damage / 2f);
                int remainingDamage = attack.damage % 2;
                if (currentArmor >= armorDamage && currentArmor != 0)
                {
                    currentArmor -= armorDamage;

                    actualArmorDamage = armorDamage;
                    actualHealthDamage = 0;
                }
                else
                {
                    remainingDamage += (armorDamage - currentArmor) * 2;
                    currentArmor = 0;
                    HitPoints -= remainingDamage;

                    actualArmorDamage = currentArmor;
                    actualHealthDamage = remainingDamage;
                }
                break;

                // Heavy does full damage to armor, then leftovers to health
            case DamageType.Heavy:
                if (currentArmor >= attack.damage && currentArmor != 0)
                {
                    currentArmor -= attack.damage;

                    actualArmorDamage = attack.damage;
                    actualHealthDamage = 0;
                }
                else
                {
                    remainingDamage = attack.damage - currentArmor;
                    currentArmor = 0;
                    HitPoints -= remainingDamage;

                    actualArmorDamage = currentArmor;
                    actualHealthDamage = remainingDamage;
                }
                break;
                // Energy does full damage to health and ignores armor
            case DamageType.Energy:
                HitPoints -= attack.damage;

                actualArmorDamage = 0;
                actualHealthDamage = attack.damage;
                break;
            default:
                break;
        }

        // Log actual damage done
        string damageTypeName = System.Enum.GetName(typeof(DamageType), attack.damageType);
        string logMessage = "";
        if (actualArmorDamage > 0)
        {
            logMessage += " " + actualArmorDamage.ToString() + " " + damageTypeName + " damage to armor";
        }

        if (actualHealthDamage > 0)
        {
            if (actualArmorDamage > 0) logMessage += " and";
            logMessage += " " + actualHealthDamage + " " + damageTypeName + " damage to health";
        }

        if (logMessage == "")
        {
            GameLog.Instance.AddMessage("Attack did 0 " + damageTypeName + " damage.");
        }
        else
        {
            GameLog.Instance.AddMessage("Attack did" + logMessage + ".");
        }

        // Invoke events
        OnUnitAttacked(attackerUberUnit, attack.damage);

        if (HitPoints <= 0)
        {
            OnUnitDestroyed(attackerUberUnit, attack.damage);
        }
    }

    public override void OnUnitAttacked(Unit attacker, int damage)
    {
        base.OnUnitAttacked(attacker, damage);

        if (unitWorldUI != null)
        {
            unitWorldUI.UpdateBars(this);
        }
    }

    public override void Move(Cell destinationCell, List<Cell> path)
    {
        if (isMoving)
        {
            return;
        }

        var totalMovementCost = path.Sum(h => h.MovementCost);
        if (MovementPoints < totalMovementCost)
        {
            return;
        }

        // MovementPoints = 0;
        MovementPoints -= totalMovementCost;

        Cell.BlocksMovement = false;
        Cell = destinationCell;
        destinationCell.BlocksMovement = true;

        if (MovementSpeed > 0)
        {
            StartCoroutine(MovementAnimation(destinationCell, path));
        }
        else
        {
            transform.position = Cell.transform.position;
            OnUnitMoved(destinationCell, path);
        }

        GetPossibleActionTargetTiles(GameMap.Instance.GetAllCellsList());
    }

    protected override IEnumerator MovementAnimation(Cell destinationCell, List<Cell> path)
    {
        isMoving = true;
        // path.Reverse();
        // path = GameMap.Instance.OrientPath(path, Cell, destinationCell);
        foreach (var cell in path)
        {
            Vector3 destination_pos = new Vector3(cell.transform.localPosition.x, transform.localPosition.y, cell.transform.localPosition.z);
            Vector3 dir = cell.transform.position - transform.position;
            dir.y = 0f;
            dir = Quaternion.Euler(0f, modelForwardRotation, 0f) * dir;

            Quaternion destination_rot = Quaternion.LookRotation(dir);
            destination_rot = Quaternion.Euler(0f, destination_rot.eulerAngles.y, 0f);

            while (transform.localPosition != destination_pos)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination_pos, Time.deltaTime * MovementSpeed);
                if (rotateSpeed == 0f)
                {
                    transform.localRotation = destination_rot;
                }
                else
                {
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, destination_rot, Time.deltaTime * rotateSpeed);
                }

                yield return 0;
            }
            transform.localRotation = destination_rot;
        }
        isMoving = false;

        OnUnitMoved(destinationCell, path);
    }

    /// <summary>
    /// Changes Unit position to destination cell instantly without considering movement restrictions. 
    /// </summary>
    /// <param name="destinationCell"></param>
    public void ForceMove(Cell destinationCell)
    {
        if (Cell != null && !IsCellBlockedByAnythingElse())
        {
            Cell.BlocksMovement = false;
        }

        Cell = destinationCell;
        destinationCell.BlocksMovement = true;

        Vector3 offset = new Vector3(0, Cell.GetCellDimensions().y, 0);
        transform.position = Cell.transform.position + offset;

        List<Cell> path = GameMap.Instance.FindLineOfSight(Cell, destinationCell);
        OnUnitMoved(destinationCell, path);
    }

    /// <summary>
    /// Only checks if cell is blocked by another unit
    /// </summary>
    /// <returns></returns>
    private bool IsCellBlockedByAnythingElse()
    {
        List<UberUnit> unitsOnCell = UnitManager.Instance.GetActiveUnitsOnCell(Cell);
        foreach (UberUnit unit in unitsOnCell)
        {
            if (unit != this)
            {
                return true;
            }
        }

        // TODO: take interactable points, cover, obstacles to account?

        return false;
    }

    public void Rotate(Vector3 dir)
    {
        StartCoroutine(SmoothRotate(dir));
    }

    private IEnumerator SmoothRotate(Vector3 dir)
    {
        dir.y = 0f;
        dir = Quaternion.Euler(0f, modelForwardRotation, 0f) * dir;

        Quaternion destination_rot = Quaternion.LookRotation(dir);
        destination_rot = Quaternion.Euler(0f, destination_rot.eulerAngles.y, 0f);

        while (transform.localRotation != destination_rot)
        {
            if (rotateSpeed == 0f)
            {
                transform.localRotation = destination_rot;
            }
            else
            {
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, destination_rot, Time.deltaTime * rotateSpeed);
            }

            yield return 0;
        }
    }

    public float GetRotateTime(Vector3 dir)
    {
        dir.y = 0f;
        dir = Quaternion.Euler(0f, modelForwardRotation, 0f) * dir;

        Quaternion destination_rot = Quaternion.LookRotation(dir);
        destination_rot = Quaternion.Euler(0f, destination_rot.eulerAngles.y, 0f);

        float deltaRotation = Mathf.Abs(destination_rot.eulerAngles.y - transform.rotation.eulerAngles.y);
        return deltaRotation / rotateSpeed;
    }

    public void ForceRotate(Vector3 dir)
    {
        dir.y = 0f;
        dir = Quaternion.Euler(0f, modelForwardRotation, 0f) * dir;

        Quaternion destination_rot = Quaternion.LookRotation(dir);
        destination_rot = Quaternion.Euler(0f, destination_rot.eulerAngles.y, 0f);

        transform.localRotation = destination_rot;
    }

    public void PlaceInLimbo(Cell limboCell)
    {
        if (Cell != null)
        {
            Cell.BlocksMovement = false;
        }
        Cell = limboCell;

        transform.position = limboCell.transform.position;
    }

    public bool IsAlive()
    {
        return HitPoints > 0;
    }

    public override void OnUnitDestroyed(Unit attacker, int damage)
    {
        base.OnUnitDestroyed(attacker, damage);

        // Spawn loot
        LootSpawner.Instance.SpawnDeathLoot(this, Cell);

        Cell.BlocksMovement = false;
        Cell.BlocksLineOfSight = false;
        Cell.IsCover = false;
        MarkAsDestroyed();
        // Destroy(gameObject);

        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
    }

    public void DestroyUnit()
    {
        HitPoints = 0;
        OnUnitDestroyed(this, TotalHitPoints);
    }

    public virtual void UpdateCoreStatsToUnit(UnitCoreData newCore)
    {
        TotalHitPoints = newCore.maxHP;
        HitPoints = (HitPoints > TotalHitPoints) ? TotalHitPoints : HitPoints;
        TotalActionPoints = newCore.maxActionPoints;
        ActionPoints = (ActionPoints > TotalActionPoints) ? TotalActionPoints : ActionPoints;
        TotalMovementPoints = newCore.movementPoints;
        MovementPoints = (MovementPoints > TotalMovementPoints) ? TotalMovementPoints : MovementPoints;
        TotalEnergy = newCore.maxEnergy;
        currentEnergy = (currentEnergy > TotalEnergy) ? TotalEnergy : currentEnergy;

        if (unitWorldUI != null)
        {
            unitWorldUI.UpdateBars(this);
        }
    }

    public bool HasRoomForEquipment(Equipment equipment)
    {
        return core.HasRoomForEquipment(equipment, equipped);
    }

    public bool HasRoomForItem(LootableItem item)
    {
        return item.UnitHasRoomForItem(this);
    }

    public virtual bool EquipItem(Equipment equipment, bool log = true)
    {
        if (HasRoomForEquipment(equipment))
        {
            equipped.Add(equipment);

            equipment.ApplyEquipEffects(this);

            if (unitWorldUI != null)
            {
                unitWorldUI.UpdateBars(this);
            }
            return true;
        }
        return false;
    }

    public List<AmmoStack> GetAmmoStacksOfType(AmmoData ammoData)
    {
        List<AmmoStack> sameAmmoType = new List<AmmoStack>();
        foreach (Equipment equipment in equipped)
        {
            if (equipment is AmmoStack)
            {
                AmmoStack ammoStack = (AmmoStack) equipment;
                if (ammoStack.ammoData == ammoData)
                {
                    sameAmmoType.Add(ammoStack);
                }
            }
        }
        return sameAmmoType;
    }

    public virtual bool EquipAmmoStack(AmmoStack ammoStack, bool log = true)
    {
        if (HasRoomForItem(ammoStack))
        {
            List<AmmoStack> sameTypeAmmo = GetAmmoStacksOfType(ammoStack.ammoData);
            if (sameTypeAmmo.Count == 0)
            {
                return EquipItem(ammoStack, false);
            }
            else
            {
                List<AmmoStack> combinedAmmo = new List<AmmoStack>();
                AmmoStack combining = ammoStack;
                foreach (AmmoStack otherAmmo in sameTypeAmmo)
                {
                    List<AmmoStack> combinedWithOther = combining.CombineWithAmmoStack(otherAmmo);
                    if (combinedWithOther == null || combinedWithOther.Count == 0)
                    {
                        Debug.LogError("AmmoStack combination calculation error");
                        return false;
                    }

                    // If combined into one stack with no leftovers
                    if (combinedWithOther.Count == 1)
                    {
                        combinedAmmo.Add(combinedWithOther[0]);
                        combining = null;
                        break;
                    }
                    else // Save the full stack and continue combining with the leftovers
                    {
                        AmmoStack fullStack = combinedWithOther.Find(a => a.ammoCount == a.ammoData.stackSize);
                        if (fullStack != null)
                        {
                            combinedAmmo.Add(fullStack);
                        }
                        combining = combinedWithOther.Find(a => a != fullStack);
                    }
                }

                // Add the last leftover if any
                if (combining != null && !combinedAmmo.Contains(combining))
                {
                    combinedAmmo.Add(combining);
                }

                foreach (AmmoStack equippedAmmo in sameTypeAmmo)
                {
                    UnequipItem(equippedAmmo);
                }

                foreach (AmmoStack combined in combinedAmmo)
                {
                    EquipItem(combined, false);
                }

                return true;
            }
        }
        return false;
    }

    public virtual Equipment UnequipItem(Equipment equipment, bool emptyWeapon = true)
    {
        if (equipped == null || equipped.Count == 0)
        {
            return null;
        }

        if (!equipped.Contains(equipment) && !Equipment.IsEmptyEquipment(equipment))
        {
            Debug.LogWarning("Trying to unequip non equipped item");
            return null;
        }

        if (activeEquipment == equipment)
        {
            activeEquipment = Equipment.EmptyEquipment();
        }

        equipment.UndoEquipEffects(this);

        equipped.Remove(equipment);

        if (equipment is Weapon && equipment.data is WeaponData && emptyWeapon)
        {
            Weapon weapon = (Weapon) equipment;
            WeaponData weaponData = (WeaponData) equipment.data;
            if (weapon.loadedAmmo > 0)
            {
                LootSpawner.Instance.SpawnAmmoStack(Cell, weapon.loadedAmmo, weaponData.ammoType);
                weapon.loadedAmmo = 0;
            }
        }

        if (unitWorldUI != null)
        {
            unitWorldUI.UpdateBars(this);
        }

        return equipment;
    }

    public virtual void DropItem(Equipment equipment)
    {
        if (UnequipItem(equipment) == equipment)
        {
            // place loot drop on tile
            LootSpawner.Instance.DropItem(Cell, equipment);
        }
    }

    public override bool TakeAction(Cell targetCell)
    {
        int actionPointCost = activeEquipment.GetActionPointCost();

        if (!CanTakeAction(actionPointCost))
        {
            return false;
        }

        if (!activeEquipment.CanTargetTile())
        {
            return false;
        }

        ActionPoints -= actionPointCost;

        Vector3 dir = targetCell.transform.position - Cell.transform.position;
        dir.y = 0f;
        Rotate(dir);

        StartCoroutine(TakeActionResolveAfterRotate(targetCell, GetRotateTime(dir)));

        return true;
    }

    private IEnumerator TakeActionResolveAfterRotate(Cell targetCell, float delay)
    {
        yield return new WaitForSeconds(delay);

        activeEquipment.Activate(this, targetCell);

        OnUnitUsedAction();
    }

    public override bool TakeAction(Unit targetUnit)
    {
        int actionPointCost = activeEquipment.GetActionPointCost();

        if (!CanTakeAction(actionPointCost))
        {
            return false;
        }

        if (!activeEquipment.CanTargetUnit())
        {
            return false;
        }

        ActionPoints -= actionPointCost;

        Vector3 dir = targetUnit.Cell.transform.position - Cell.transform.position;
        dir.y = 0f;
        Rotate(dir);

        StartCoroutine(TakeActionResolveAfterRotate(targetUnit, GetRotateTime(dir)));

        return true;
    }

    private IEnumerator TakeActionResolveAfterRotate(Unit targetUnit, float delay)
    {
        yield return new WaitForSeconds(delay);

        activeEquipment.Activate(this, targetUnit);

        OnUnitUsedAction();
    }

    public override bool TakeAction()
    {
        int actionPointCost = activeEquipment.GetActionPointCost();

        if (!CanTakeAction(actionPointCost))
        {
            return false;
        }

        if (!activeEquipment.CanActivateWithoutTarget())
        {
            return false;
        }

        activeEquipment.Activate(this);

        ActionPoints -= actionPointCost;

        OnUnitUsedAction();

        return true;
    }

    private bool CanTakeAction(int actionPointCost)
    {
        if (isMoving)
        {
            return false;
        }
        if (activeEquipment == null)
        {
            return false;
        }

        if (ActionPoints < actionPointCost)
        {
            return false;
        }

        if (!activeEquipment.CanActivate(this))
        {
            return false;
        }

        return true;
    }

    public int GetWeaponHitChanceModifiers(List<string> synergyTags)
    {
        int mod = 0;

        foreach (Equipment equipment in equipped)
        {
            mod += equipment.GetHitChanceModifier(synergyTags);
        }

        foreach (Buff buff in Buffs)
        {
            mod += buff.GetHitChanceModifier();
        }

        return mod;
    }

    public string GetWeaponHitChanceModifiersDescription(List<string> synergyTags)
    {
        string desc = "";

        foreach (Equipment equipment in equipped)
        {
            int bonus = equipment.GetHitChanceModifier(synergyTags);
            if (bonus != 0)
            {
                desc += equipment.GetItemName() + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        foreach (Buff buff in Buffs)
        {
            int bonus = buff.GetHitChanceModifier();
            if (bonus != 0)
            {
                desc += buff.buffSourceName + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        return desc;
    }

    public int GetWeaponCriticalChanceModifiers(List<string> synergyTags)
    {
        int mod = 0;

        foreach (Equipment equipment in equipped)
        {
            mod += equipment.GetCriticalChanceModifier(synergyTags);
        }

        foreach (Buff buff in Buffs)
        {
            mod += buff.GetCriticalChanceModifier();
        }

        return mod;
    }

    public string GetWeaponCriticalChanceModifiersDescription(List<string> synergyTags)
    {
        string desc = "";

        foreach (Equipment equipment in equipped)
        {
            int bonus = equipment.GetCriticalChanceModifier(synergyTags);
            if (bonus != 0)
            {
                desc += equipment.GetItemName() + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        foreach (Buff buff in Buffs)
        {
            int bonus = buff.GetCriticalChanceModifier();
            if (bonus != 0)
            {
                desc += buff.buffSourceName + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        return desc;
    }

    public int GetWeaponDamageModifiers(List<string> synergyTags)
    {
        int mod = 0;

        foreach (Equipment equipment in equipped)
        {
            mod += equipment.GetDamageModifier(synergyTags);
        }

        foreach (Buff buff in Buffs)
        {
            mod += buff.GetDamageModifier();
        }

        return mod;
    }

    public string GetWeaponDamageModifiersDescription(List<string> synergyTags)
    {
        string desc = "";

        foreach (Equipment equipment in equipped)
        {
            int bonus = equipment.GetDamageModifier(synergyTags);
            if (bonus != 0)
            {
                desc += equipment.GetItemName() + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        foreach (Buff buff in Buffs)
        {
            int bonus = buff.GetDamageModifier();
            if (bonus != 0)
            {
                desc += buff.buffSourceName + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        return desc;
    }

    public int GetDefenceHitChanceModifiers()
    {
        int mod = 0;

        foreach (Equipment equipment in equipped)
        {
            mod += equipment.GetDefenceHitChanceModifier();
        }

        foreach (Buff buff in Buffs)
        {
            mod += buff.GetDefenceHitChanceModifier();
        }

        return mod;
    }

    public string GetDefenceHitChanceModifiersDescription()
    {
        string desc = "";

        foreach (Equipment equipment in equipped)
        {
            int bonus = equipment.GetDefenceHitChanceModifier();
            if (bonus != 0)
            {
                desc += equipment.GetItemName() + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        foreach (Buff buff in Buffs)
        {
            int bonus = buff.GetDefenceHitChanceModifier();
            if (bonus != 0)
            {
                desc += buff.buffSourceName + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        return desc;
    }

    public int GetDefenceCriticalChanceModifiers()
    {
        int mod = 0;

        foreach (Equipment equipment in equipped)
        {
            mod += equipment.GetDefenceCriticalChanceModifier();
        }

        foreach (Buff buff in Buffs)
        {
            mod += buff.GetDefenceCriticalChanceModifier();
        }

        return mod;
    }

    public string GetDefenceCriticalChanceModifiersDescription()
    {
        string desc = "";

        foreach (Equipment equipment in equipped)
        {
            int bonus = equipment.GetDefenceCriticalChanceModifier();
            if (bonus != 0)
            {
                desc += equipment.GetItemName() + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        foreach (Buff buff in Buffs)
        {
            int bonus = buff.GetDefenceCriticalChanceModifier();
            if (bonus != 0)
            {
                desc += buff.buffSourceName + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        return desc;
    }

    public int GetDefenceDamageModifiers()
    {
        int mod = 0;

        foreach (Equipment equipment in equipped)
        {
            mod += equipment.GetDefenceDamageModifier();
        }

        foreach (Buff buff in Buffs)
        {
            mod += buff.GetDefenceDamageModifier();
        }

        return mod;
    }

    public string GetDefenceDamageModifiersDescription()
    {
        string desc = "";

        foreach (Equipment equipment in equipped)
        {
            int bonus = equipment.GetDefenceDamageModifier();
            if (bonus != 0)
            {
                desc += equipment.GetItemName() + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        foreach (Buff buff in Buffs)
        {
            int bonus = buff.GetDefenceDamageModifier();
            if (bonus != 0)
            {
                desc += buff.buffSourceName + ": " + bonus.ToStringWithSign() + "%\n";
            }
        }

        return desc;
    }

    /// <summary>
    /// Gets all possible target tiles for attack range
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    public virtual HashSet<Cell> GetPossibleActionTargetTiles(List<Cell> cells)
    {
        List<Cell> cellsInAttackRange = cells.FindAll(c => GameMap.Instance.GetLineDistanceBetweenCells(c, Cell) <= activeEquipment.GetRange());

        cachedAttackPaths = new Dictionary<Cell, List<Cell>>();
        List<List<Cell>> attackPaths = new List<List<Cell>>();
        foreach (Cell cellInAttackRange in cellsInAttackRange)
        {
            List<Cell> attackPath = GameMap.Instance.FindLineOfSight(cellsInAttackRange, Cell, cellInAttackRange);

            if (!GameMap.Instance.IsLineOfSightPathBlocked(attackPath, Cell, cellInAttackRange) ||
                !activeEquipment.RequiresLineOfSight())
            {
                cachedAttackPaths.Add(cellInAttackRange, attackPath);
            }
        }

        return new HashSet<Cell>(cachedAttackPaths.Keys);
    }

    public override bool IsUnitAttackable(Unit other, Cell sourceCell)
    {
        if (other == null) return false;
        if (other is UberUnit)
        {
            UberUnit otherUber = (UberUnit)other;
            if (!otherUber.IsAlive())
            {
                return false;
            }
        }

        if (sourceCell != Cell || cachedAttackPaths == null)
        {
            List<Cell> attackPath = GameMap.Instance.FindLineOfSight(sourceCell, other.Cell);

            if (activeEquipment.RequiresLineOfSight() &&
                GameMap.Instance.IsLineOfSightPathBlocked(attackPath, sourceCell, other.Cell))
            {
                return false;
            }

            if (attackPath.Count > activeEquipment.GetRange())
            {
                return false;
            }

            return true;
        }

        return cachedAttackPaths.Keys.Contains(other.Cell);
    }

    public override bool IsCellAttackable(Cell otherCell, Cell sourceCell)
    {
        if (sourceCell != Cell || cachedAttackPaths == null)
        {
            List<Cell> attackPath = GameMap.Instance.FindLineOfSight(sourceCell, otherCell);

            if (activeEquipment.RequiresLineOfSight() &&
                GameMap.Instance.IsLineOfSightPathBlocked(attackPath, sourceCell, otherCell))
            {
                return false;
            }

            if (attackPath.Count > activeEquipment.GetRange())
            {
                return false;
            }

            return true;
        }

        return cachedAttackPaths.Keys.Contains(otherCell);
    }

    public virtual List<Cell> FindActionTargetingPath(List<Cell> cells, Cell targetCell)
    {
        if (cachedAttackPaths != null && cachedAttackPaths.Keys.Contains(targetCell))
        {
            return cachedAttackPaths[targetCell];
        }
        else
        {
            return GameMap.Instance.FindLineOfSight(cells, Cell, targetCell);
        }
    }

    public override bool IsCellTraversable(Cell cell)
    {
        return !cell.BlocksMovement;
    }

    public bool IsCellTraversableCarelessly(Cell cell)
    {
        DoorPoint door = InteractablePointManager.Instance.GetDoorPointOnCell(cell);
        if (door != null)
        {
            return true;
        }

        UberUnit otherUnit = UnitManager.Instance.GetActiveUnitOnCell(cell);
        if (otherUnit != null)
        {
            return true;
        }

        return !cell.BlocksMovement;
    }

    public override bool IsCellMovableTo(Cell cell)
    {
        return !cell.BlocksMovement;
    }

    public bool CanMoveToCell(Cell destinationCell)
    {
        if (destinationCell == Cell)
        {
            return true;
        }

        List<Cell> path = FindPath(GameMap.Instance.GetAllCellsList(), destinationCell);
        if (path == null || path.Count == 0)
        {
            return false;
        }

        return GameMap.Instance.GetMovementPathCost(path, Cell, destinationCell) <= MovementPoints;
    }

    /// <summary>
    /// Method returns all cells that the unit is capable of moving to.
    /// </summary>
    public HashSet<Cell> GetCarelessAvailableDestinations(List<Cell> cells)
    {
        carelessCachedPaths = new Dictionary<Cell, List<Cell>>();

        Dictionary<Cell, List<Cell>> paths = CacheCarelessPaths(cells);
        foreach (Cell key in paths.Keys)
        {
            if (!IsCellMovableTo(key)) continue;

            List<Cell> path = paths[key];
            path = GameMap.Instance.OrientPath(path, Cell, key);

            int pathCost = GameMap.Instance.GetMovementPathCost(path, Cell, key);
            if (pathCost <= MovementPoints)
            {
                carelessCachedPaths.Add(key, path);
            }
        }
        return new HashSet<Cell>(carelessCachedPaths.Keys);
    }

    private Dictionary<Cell, List<Cell>> CacheCarelessPaths(List<Cell> cells)
    {
        var edges = GetCarelessGraphEdges(cells);
        var paths = _pathfinder.findAllPaths(edges, Cell);
        return paths;
    }

    public List<Cell> FindPathCarelessly(List<Cell> cells, Cell destinationCell)
    {
        if (carelessCachedPaths != null && carelessCachedPaths.ContainsKey(destinationCell))
        {
            return carelessCachedPaths[destinationCell];
        }
        else
        {
            List<Cell> path = _fallbackPathfinder.FindPath(GetCarelessGraphEdges(cells), Cell, destinationCell);
            return GameMap.Instance.OrientPath(path, Cell, destinationCell);
        }
    }

    /// <summary>
    /// Method returns graph representation of cell grid for pathfinding. 
    /// Careless so includes temporary obstacles like doors and other units.
    /// </summary>
    protected virtual Dictionary<Cell, Dictionary<Cell, int>> GetCarelessGraphEdges(List<Cell> cells)
    {
        Dictionary<Cell, Dictionary<Cell, int>> ret = new Dictionary<Cell, Dictionary<Cell, int>>();
        foreach (var cell in cells)
        {
            if (IsCellTraversableCarelessly(cell) || cell.Equals(Cell))
            {
                ret[cell] = new Dictionary<Cell, int>();
                foreach (var neighbour in cell.GetNeighbours(cells).FindAll(IsCellTraversableCarelessly))
                {
                    ret[cell][neighbour] = neighbour.MovementCost;
                }
            }
        }
        return ret;
    }

    public void ModifyMovementPoints(int amount)
    {
        TotalMovementPoints += amount;
        MovementPoints += amount;
        if (MovementPoints < 0) MovementPoints = 0;
        if (MovementPoints > TotalMovementPoints) MovementPoints = TotalMovementPoints;
    }

    public void ModifyArmorPoints(int totalArmorChange, int currentArmorChange)
    {
        // TotalArmor += totalArmorChange;
        currentArmor += currentArmorChange;
        if (currentArmor < 0) currentArmor = 0;
        if (currentArmor > TotalArmor) currentArmor = TotalArmor;
    }

    public bool IsVisible()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.isVisible;
        }

        return false;
    }

    public override void DealDamage(Unit other)
    {
        Debug.LogError("Wrong method used to deal damage. Use DealDamage with WeaponAttack parameter");
    }

    protected override void Defend(Unit other, int damage)
    {
        Debug.LogError("Wrong method used to defend. Use overload with WeaponAttack parameter.");
    }
}
