using UnityEngine;

public class Unit : SelectableEntity
{
    public enum ECategory
    {
        Light,
        Medium,
        Heavy
    }

    [SerializeField] private ECategory Category;
    [SerializeField] private int Cost = 1;
    [SerializeField] private int Hp;
    [SerializeField] private int MaxHp = 100;

    public delegate void OnDeathEventHandler(object sender);
    public event OnDeathEventHandler OnDeadEvent;

    public delegate void OnPathFollowedHandler();
    public event OnPathFollowedHandler OnPathFollowedEvent;

    private Movement _movement;
    private UnitFactory _factory;
    private UnitGroup _groupFollowed;

    private float lastDamageDate = 0f;
    private GameObject hitFX;

    private bool isInitialized = false;
    private bool isAlive = true;
    private bool isPathFollowDone = false;


    public bool IsAlive { get { return isAlive;} }

    public ECategory GetCategory { get { return Category; } }
    public int GetCost { get { return Cost; } }
    public Movement Movement { get { return _movement; } }
    public UnitGroup GroupFollowed { get { return _groupFollowed; } }

    public void Init(Team _team)
    {
        if (isInitialized)
            return;

        if (_team == Team.Green)
            GetComponent<Rigidbody>().constraints -= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        team = _team;
        Hp = MaxHp;
        OnDeadEvent += Unit_OnDead;

        isInitialized = true;
    }

    override protected void Awake()
    {
        base.Awake();
        hitFX = transform.Find("HitFX").gameObject;
        _movement = GetComponent<Movement>();
        if (_movement == null)
            Debug.LogError("No Movement Script on [" + name + "] !");

        OnPathFollowedEvent += () =>
        {
            _movement.Steering.ClearPath();
            isPathFollowDone = true;
            if (_groupFollowed)
                _groupFollowed.UnitInFormation();
        };
    }

    private void Unit_OnDead(object sender)
    {
        isAlive = false;
        Destroy(gameObject);
    }

    private void Update ()
    {
        if (team == Team.Green)
        {
            _movement.Steering.Wander();
            return;
        }

        if (_groupFollowed)
        {
            if (isPathFollowDone)
                _movement.Steering.Seek(GroupFollowed.GetFormationPosition(this));
        }
        else
            _movement.Stop();
    }

    public void GoTo(Vector3 pos)
    {
        isPathFollowDone = false;
        _movement.Steering.SearchPathTo(pos, OnPathFollowedEvent);
    }

    public void AddDamages(int damages)
    {
        Hp -= damages;
        if (Hp <= 0)
        {
            OnDeadEvent(this);
        }
    }

    public void GoToGroupFormationPosition(UnitGroup group, Vector3 formationPosition)
    {
        _groupFollowed = group;
        GoTo(formationPosition);
    }

}
