using System.Collections.Generic;
using UnityEngine;
using Navigation;

public class UnitGroup : MonoBehaviour
{
    private Vector3 _targetPos = Vector3.zero;

    private GroupFormation _groupFormation;
    private List<Unit> _unitList = new List<Unit>();
    private float _maxSpeed = 10f;
    private int _nbUnitInPlace = 0;

    public List<Unit> UnitList { get { return _unitList; } }
    public float MaxSpeed { get { return _maxSpeed; } }

    public void Setup(Vector3 targetPos, List<Unit> newUnitList, GroupFormation.Type formationType)
    {
        _targetPos = targetPos;
        _unitList = new List<Unit>(newUnitList);

        _groupFormation = GetComponent<GroupFormation>();
        _groupFormation.SetUpFormation(formationType, _unitList.Count);

        transform.position = FindCentroid();
        _maxSpeed = ComputeSpeed();
        GetComponent<Movement>().UpdateGroupSpeed();

        foreach (Unit unit in _unitList)
            unit.GoToGroupFormationPosition(this, transform.position);
    }

    public void ChangeTargetPos(Vector3 targetPos)
    {
        _targetPos = targetPos;
        CancelInvoke("DestroyMe");

        if (_nbUnitInPlace == _unitList.Count)
            GetComponent<Steering>().SearchPathTo(_targetPos);
    }

    public void ChangeFormation(GroupFormation.Type type)
    {
        _groupFormation.ChangeFormation(type);
    }

    public Vector3 GetFormationPosition(Unit unit)
    {
        int unitIdx = _unitList.IndexOf(unit);

        return _groupFormation.GetPositionOfUnitFromIndex(unitIdx);
    }

    public void UnitInFormation()
    {
        _nbUnitInPlace++;

        if (_nbUnitInPlace == _unitList.Count)
            GetComponent<Steering>().SearchPathTo(_targetPos);
    }

    public bool HasSameUnits(List<Unit> unitsToCompare)
    {
        if (unitsToCompare.Count != _unitList.Count)
            return false;

        foreach(Unit unit in unitsToCompare)
        {
            if (_unitList.IndexOf(unit) == -1) // if unit not found
                return false;
        }

        return true;
    }

    public void MayDestroy()
    {
        Invoke("DestroyMe", 2f);
    }

    private void DestroyMe()
    {
        Destroy(gameObject);
    }

    private Vector3 FindCentroid()
    {
        Vector3 centroid = ComputeCentroid();

        TileNavGraph navGraph = TileNavGraph.Instance;
        bool result = navGraph.IsNodeWalkable(navGraph.GetNode(centroid));

        if (result)
            return centroid;

        Vector3 dirToTarget = (_targetPos - centroid).normalized;
        return navGraph.GetNearestWalkableNodeInDirection(centroid, dirToTarget).Position;
    }

    private Vector3 ComputeCentroid()
    {
        Vector3 vectorSum = Vector3.zero;
        foreach (Unit unit in _unitList)
        {
            vectorSum += unit.transform.position;
        }

        return vectorSum / _unitList.Count;
    }

    private float ComputeSpeed()
    {
        float speedTotal = float.MaxValue;
        foreach(Unit unit in _unitList)
        {
            if (unit.Movement.MaxSpeed < speedTotal)
                speedTotal = unit.Movement.MaxSpeed;
        }
        return speedTotal;
    }
}
