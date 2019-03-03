using UnityEngine;

public class UnitFactory : SelectableEntity
{
    [SerializeField]
    private int NbSpawnSlots = 10;
    [SerializeField]
    private int SpawnRadius = 6;
    [SerializeField]
    private float BuildDuration = 1.5f;
    [SerializeField]
    private GameObject UnitPrefab;

    public delegate void UnitBuiltEventHandler(Unit unit);
    public event UnitBuiltEventHandler OnUnitBuilt;

    private bool isBuilding = false;
    private float endBuildDate = 0f;

    private int spawnCount = 0;

    private int unitCost = -1;
    public int UnitCost
    {
        get
        {
            if (unitCost < 0)
                unitCost = UnitPrefab.GetComponent<Unit>().GetCost;
            return unitCost;
        }
    }

    public float GetBuildDuration { get { return BuildDuration; } }

    public bool StartBuildUnit()
    {
        if (isBuilding)
            return false;

        isBuilding = true;
        endBuildDate = Time.time + BuildDuration;
        return true;
    }

    Unit BuildUnit()
    {
        isBuilding = false;

        GameObject unitInst = Instantiate(UnitPrefab);
        Unit newUnit = unitInst.GetComponent<Unit>();
        newUnit.Init(GetTeam);
        //newUnit.OnDeadEvent += OnUnitDead;
        //unitList.Add(newUnit);

        spawnCount++;
        int nbUnits = spawnCount % NbSpawnSlots;
        unitInst.name = unitInst.name.Replace("(Clone)", "_" + spawnCount.ToString());
        // compute simple spawn position around the factory
        float angle = Mathf.PI * 2 / NbSpawnSlots * nbUnits;
        unitInst.transform.position = transform.position + new Vector3(SpawnRadius * Mathf.Cos(angle), 0f, SpawnRadius * Mathf.Sin(angle));
        RaycastHit raycastInfo;
        Ray ray = new Ray(unitInst.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out raycastInfo, 10f, 1 << LayerMask.NameToLayer("Floor")))
            unitInst.transform.position = raycastInfo.point;

        return newUnit;
    }

    private void Update()
    {
        if (isBuilding && Time.time > endBuildDate)
            OnUnitBuilt(BuildUnit());
    }
}
