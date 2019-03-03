using UnityEngine;

public class Movement : MonoBehaviour
{
    //MEMBER
    [SerializeField] private float _maxSpeed = 10f;
    [SerializeField] private float _offsetPosY = 1f;

    private Unit _unit;
    private Steering _steering;

    private Vector3 _velocity = Vector3.zero;
    private float _rotation = 0f;

    //PROPERTIES
    public float MaxSpeed { get { return _maxSpeed; } }
    public float OffsetPosY { get { return _offsetPosY; } }

    public Vector3 Velocity { get { return _velocity; } }
    public float Rotation { get { return _rotation; } }


    public Steering Steering { get { return _steering; } }

    //METHOD
    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _steering = GetComponent<Steering>();
    }

    private void Start()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position + Vector3.up * 10, Vector3.down, out hitInfo, 100, 1 << LayerMask.NameToLayer("Floor")))
            transform.position = hitInfo.point + Vector3.up * _offsetPosY;
    }

    public void Stop()
	{
		_velocity = Vector3.zero;
    }

    protected float GetOrientationFromDirection(Vector3 direction)
    {
        if (direction.magnitude > 0)
            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        
        return _rotation;
    }

    public void UpdateGroupSpeed()
    {
        if (gameObject.tag != "Commander")
            return;

        _maxSpeed = GetComponent<UnitGroup>().MaxSpeed;
    }

    private void FixedUpdate ()
	{
        if (_unit)
            if (!_unit.IsAlive)
                return;

        _steering.Compute();

        // Apply Steering and truncate to max speed
        _velocity = Vector3.ClampMagnitude(_velocity + _steering.Velocity, _maxSpeed);
        _rotation = GetOrientationFromDirection(_velocity);

        // Update velocity and rotation
        transform.position += _velocity * Time.deltaTime;
        transform.eulerAngles = Vector3.up * _rotation;

        _steering.ResetForces();
    }

    private void OnDrawGizmos()
	{
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, _steering.Velocity);
    }
}
