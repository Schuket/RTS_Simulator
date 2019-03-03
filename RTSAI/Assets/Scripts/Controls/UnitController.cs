using System.Collections.Generic;
using UnityEngine;

// points system for units creation (Ex : light units = 1 pt, medium = 2pts, heavy = 3 pts)
// max points can be increased by capturing map points
public class UnitController : MonoBehaviour
{
    [SerializeField]
    protected Team team;
    public Team GetTeam { get { return team; } }

    [SerializeField]
    protected int StartingBuildPoints = 15;
    protected int totalBuildPoints = 0;
    public int TotalBuildPoints { get { return totalBuildPoints; } }

    protected UnitFactory currentFactory;
    protected List<Unit> unitList = new List<Unit>();
    protected List<Unit> selectedUnitList = new List<Unit>();

    [SerializeField]
    protected GameObject groupPrefab;
    protected UnitGroup _lastGroupCreated = null;
    [SerializeField]
    protected GroupFormation.Type _defaultFormationType = GroupFormation.Type.ARROW;


    #region unit methods
    protected void UnselectAllUnits()
    {
        _lastGroupCreated = null;
        foreach (Unit unit in selectedUnitList)
            unit.SetSelected(false);
        selectedUnitList.Clear();
    }

    protected void SelectAllUnits()
    {
        foreach (Unit unit in unitList)
            unit.SetSelected(true);

        selectedUnitList.Clear();
        selectedUnitList.AddRange(unitList);
    }

    protected void SelectAllUnitsByCategory(Unit.ECategory cat)
    {
        UnselectAllUnits();
        selectedUnitList = unitList.FindAll(delegate (Unit unit)
            {
                return unit.GetCategory == cat;
            }
        );
        foreach(Unit unit in selectedUnitList)
        {
            unit.SetSelected(true);
        }
    }

    protected void SelectUnitList(List<Unit> units)
    {
        foreach (Unit unit in units)
            unit.SetSelected(true);
        selectedUnitList.AddRange(units);
    }

    protected void SelectUnit(Unit unit)
    {
        unit.SetSelected(true);
        selectedUnitList.Add(unit);
    }

    protected void UnseletecUnit(Unit unit)
    {
        unit.SetSelected(false);
        selectedUnitList.Remove(unit);
    }

    private void AddUnit(Unit unit)
    {
        //FIX for the event difficulties
        if (unitList.IndexOf(unit) != -1)
            return;

        unit.OnDeadEvent += (object sender) =>
        {
            totalBuildPoints += unit.GetCost;
        };
        unitList.Add(unit);
    }

    public void AddBuildPoints(int points)
    {
        totalBuildPoints += points;
    }
    #endregion

    #region factory methods
    protected void SelectFactory(UnitFactory factory)
    {
        currentFactory = factory;
        currentFactory.SetSelected(true);
        UnselectAllUnits();
    }

    protected void UnselectCurrentFactory()
    {
        if (currentFactory != null)
            currentFactory.SetSelected(false);
        currentFactory = null;
    }

    protected void RequestFactoryBuild()
    {
        if (currentFactory == null)
            return;

        if (totalBuildPoints < currentFactory.UnitCost)
            return;


        // This Event is being called multiple time , cause each time you build a unit you add this function to the event :x
        currentFactory.OnUnitBuilt += (Unit unit) =>
        {
            if (unit != null)
                AddUnit(unit);
        };
        bool result = currentFactory.StartBuildUnit();

        //Loose build Points only if the unit can actually by created
        if (result == true)
            totalBuildPoints -= currentFactory.UnitCost;
    }

    #endregion

    virtual protected void Start ()
    {
        totalBuildPoints = StartingBuildPoints;
    }

    virtual protected void Update()
    {

    }
}
