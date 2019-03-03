using System.Collections.Generic;
using UnityEngine;
using Navigation;

public class Steering : MonoBehaviour
{
    //Editor
    [SerializeField] private float MAX_FORCES = 1f;

    [Header("Arrival")]
    [SerializeField] private float SEEK_RADIUS = 2f;

    [Header("Wander")]
    [SerializeField] private float CIRCLE_DISTANCE = 10f;
    [SerializeField] private float CIRCLE_RADIUS = 8f;
    [SerializeField] private float ANGLE_VARIATION = 45f;
    private float _wanderAngle = -90f;

    [Header("CollisionAvoidance")]
    [SerializeField] private float MAX_AVOID_FORCE = 30f;

    [Header("PathFollow")]
    [SerializeField] private float RADIUS = 3f;

    // Member
    List<Vector3> PathPos = new List<Vector3>();
    List<Node> _path = new List<Node>();
    int _currNodeID = 0;

    private Movement _movement;
    private AStar _aStar = new AStar();

    private Vector3 _velocity = Vector3.zero;
    private Vector3 _collisionSteering = Vector3.zero;

    //Property
    public Vector3 Velocity { get { return _velocity; } }

    //Event
    public delegate void OnPathFoundHandler();
    public event OnPathFoundHandler OnPathFoundEvent;

    Unit.OnPathFollowedHandler onPathFollowed;

    //Method
    private void Awake()
    {
        _movement = GetComponent<Movement>();
        OnPathFoundEvent += () =>
        {
            _path = _aStar.Path;
            //Debug
            for(int i = 0; i < 10; i++)
            {
                PathPos.Add(new Vector3(_aStar.Path.Count,0f,0f));
                PathPos.Add(_aStar.Path[i].Position);
            }
        };
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 dir;
        if (collision.collider is TerrainCollider)
            dir = collision.contacts[0].normal;
        else
            dir = collision.contacts[0].point - collision.gameObject.transform.position;

        dir.y = 0f;
        _collisionSteering = dir * MAX_AVOID_FORCE;
    }

    private void OnCollisionExit(Collision collision)
    {
        _collisionSteering = Vector3.zero;
    }

    //Public Method
    public void Compute()
    {
        _velocity += _collisionSteering;
        _velocity += ComputePathFollow();

        _velocity = Vector3.ClampMagnitude(_velocity, MAX_FORCES);
        // _velocity /= GetComponent<Rigidbody>().mass;
    }

    public void ResetForces()
    {
        _velocity = Vector3.zero;
    }

    public void Seek(Vector3 targetPos)
    {
        _velocity += ComputeSeek(targetPos);
    }

    public void Wander()
    {
        _velocity += ComputeWander();
    }

    public void SearchPathTo(Vector3 pos, Unit.OnPathFollowedHandler onPathFollowedEvent = null)
    {
        ClearPath();
        _aStar.SearchPath(transform.position, pos, OnPathFoundEvent);

        if (onPathFollowedEvent != null)
        {
            if (onPathFollowed != onPathFollowedEvent)
                onPathFollowed += onPathFollowedEvent;
        }
    }

    //Private Method
    #region ComputeSteeringBehaviour

    private Vector3 ComputeSeek(Vector3 targetPos)
    {
        Vector3 desiredVelocity = targetPos - transform.position;
        float dist = desiredVelocity.magnitude;

        if (dist < SEEK_RADIUS)
            desiredVelocity = Vector3.Normalize(desiredVelocity) * _movement.MaxSpeed * (dist / SEEK_RADIUS);
        else
            desiredVelocity = Vector3.Normalize(desiredVelocity) * _movement.MaxSpeed;

        return desiredVelocity - _movement.Velocity;
    }

    private Vector3 ComputeWander()
    {
        Vector3 circleCenter = _movement.Velocity.normalized;
        circleCenter *= CIRCLE_DISTANCE;

        Vector3 displacement = GetVectorFromAngle(_wanderAngle);
        displacement *= CIRCLE_RADIUS;

        _wanderAngle += Random.value * ANGLE_VARIATION - ANGLE_VARIATION * 0.5f;

        // steering force
        return (circleCenter + displacement);
    }

    private Vector3 ComputePathFollow()
    {
        Vector3 target = Vector3.zero;

        if (_path.Count != 0)
        {
            //get currNode with the right Y 
            target = _path[_currNodeID].Position + (Vector3.up *_movement.OffsetPosY);

            if (Vector3.Distance(transform.position, target) <= RADIUS)
            {
                _currNodeID++;

                if (_currNodeID >= _path.Count)
                {
                    _currNodeID = _path.Count - 1;

                    if (gameObject.tag == "Commander")
                        gameObject.GetComponent<UnitGroup>().MayDestroy();

                    if (Vector3.Distance(transform.position, target) <= 1f)
                    {
                        if (onPathFollowed != null)
                            onPathFollowed();
                    }
                   
                }
            }
        }

        return target != Vector3.zero ? ComputeSeek(target) : Vector3.zero;
    }

    public void ClearPath()
    {
        _path.Clear();
        _currNodeID = 0;
    }
    #endregion

    #region Tools

    private Vector3 GetVectorFromAngle(float angle)
    {
        angle *= Mathf.Deg2Rad;
        float posX = Mathf.Cos(angle);
        float posZ = Mathf.Sin(angle);

        return new Vector3(posX, 0f, posZ);
    }

    #endregion
}
