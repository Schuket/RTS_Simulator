using UnityEngine;
using Navigation;

public class PlayerController : UnitController
{

    [SerializeField]
    private GameObject TargetCursorPrefab = null;
    private GameObject targetCursor = null;

    delegate void InputEventHandler();
    event InputEventHandler OnMouseClicked;
    event InputEventHandler OnDeletePressed;
    event InputEventHandler OnSelectAllPressed;
    event InputEventHandler OnUnSelectAllPressed;
    event InputEventHandler OnLightCatPressed;
    event InputEventHandler OnMediumCatPressed;
    event InputEventHandler OnHeavyCatPressed;
    event InputEventHandler OnArrowFormationPressed;
    event InputEventHandler OnSquareFormationPressed;
    event InputEventHandler OnCircleFormationPressed;

    private GameObject GetTargetCursor()
    {
        if (targetCursor == null)
            targetCursor = Instantiate(TargetCursorPrefab);
        return targetCursor;
    }

    override protected void Start()
    {
        base.Start();

        OnMouseClicked += () =>
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int factoryLayer = 1 << LayerMask.NameToLayer("Factory");
            int unitLayer = 1 << LayerMask.NameToLayer("Unit");
            int floorLayer = 1 << LayerMask.NameToLayer("Floor");

            RaycastHit raycastInfo;
            // factory selection
            if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, factoryLayer))
            {
                UnitFactory factory = raycastInfo.transform.GetComponent<UnitFactory>();
                if (factory != null &&  factory.GetTeam == team)
                {
                    if (currentFactory == factory)
                    {
                        RequestFactoryBuild();
                    }
                    else
                    {
                        UnselectCurrentFactory();
                        SelectFactory(factory);
                    }
                }
            }
            // unit selection / unselection
            else if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, unitLayer))
            {
                bool isShiftBtPressed = Input.GetKey(KeyCode.LeftShift);
                bool isCtrlBtPressed = Input.GetKey(KeyCode.LeftControl);

                UnselectCurrentFactory();

                Unit selectedUnit = raycastInfo.transform.GetComponent<Unit>();
                if (selectedUnit != null && selectedUnit.GetTeam == team)
                {
                    if (isShiftBtPressed)
                    {
                        UnseletecUnit(selectedUnit);
                    }
                    else if (isCtrlBtPressed)
                    {
                        SelectUnit(selectedUnit);
                    }
                    else
                    {
                        
                        UnselectAllUnits();
                        SelectUnit(selectedUnit);
                    }
                }
            }
            // unit move target
            else if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, floorLayer))
            {
                UnselectCurrentFactory();

                if (selectedUnitList.Count == 0)
                    return;

                Vector3 newPos = raycastInfo.point;
                Vector3 targetPos = newPos;
                targetPos.y += 0.1f;
                GetTargetCursor().transform.position = targetPos;
                Node targetNode = TileNavGraph.Instance.GetNode(targetPos);

                if (TileNavGraph.Instance.IsPosValid(newPos) && TileNavGraph.Instance.IsNodeWalkable(targetNode))
                {
                    if (_lastGroupCreated)
                    {
                        if(_lastGroupCreated.HasSameUnits(selectedUnitList))
                        {
                            _lastGroupCreated.ChangeTargetPos(newPos);
                            return;
                        }
                    }

                    _lastGroupCreated = GameObject.Instantiate(groupPrefab).GetComponent<UnitGroup>();
                    _lastGroupCreated.Setup(newPos, selectedUnitList, _defaultFormationType);
                }
            }
        };

        OnDeletePressed += () =>
        {
            foreach (Unit unit in unitList)
            {
                unit.AddDamages(100);
            }
            unitList.Clear();
        };

        OnSelectAllPressed += () =>
        {
            SelectAllUnits();
        };

        OnUnSelectAllPressed += () =>
        {
            UnselectAllUnits();
        };

        OnLightCatPressed += () =>
        {
            SelectAllUnitsByCategory(Unit.ECategory.Light);
        };

        OnMediumCatPressed += () =>
        {
            SelectAllUnitsByCategory(Unit.ECategory.Medium);
        };

        OnHeavyCatPressed += () =>
        {
            SelectAllUnitsByCategory(Unit.ECategory.Heavy);
        };

        OnArrowFormationPressed += () =>
        {
            _defaultFormationType = GroupFormation.Type.ARROW;
            if (_lastGroupCreated)
                _lastGroupCreated.ChangeFormation(_defaultFormationType);
        };

        OnSquareFormationPressed += () =>
        {
            _defaultFormationType = GroupFormation.Type.SQUARE;
            if (_lastGroupCreated)
                _lastGroupCreated.ChangeFormation(_defaultFormationType);
        };

        OnCircleFormationPressed += () =>
        {
            _defaultFormationType = GroupFormation.Type.CIRCLE;
            if (_lastGroupCreated)
                _lastGroupCreated.ChangeFormation(_defaultFormationType);
        };
    }

    override protected void Update ()
    {
        if (Input.GetMouseButtonDown(0))
            OnMouseClicked();

        if (Input.GetKeyDown(KeyCode.Delete))
            OnDeletePressed();

        if (Input.GetKeyDown(KeyCode.A))
            OnSelectAllPressed();

        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
            OnUnSelectAllPressed();

        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
            OnLightCatPressed();

        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
            OnMediumCatPressed();

        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
            OnHeavyCatPressed();

        if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
            OnArrowFormationPressed();

        if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
            OnSquareFormationPressed();

        if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
            OnCircleFormationPressed();
    }
}
