using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Team
{
    Red,
    Blue
}
public enum DroneState
{
    Wander,
    Shase,
    Attack
}

public class DroneSimpleScr : MonoBehaviour
{
    public Team Team => _team;
    [SerializeField] private Team _team;
    [SerializeField] private LayerMask _layerMask;
    private float _attackRange = 3f;
    private float _rayDisntace = 5.0f;
    private float _stoppingDistance = 5f;

    private Vector3 _destination;
    private Quaternion _desiredRotation;
    private Vector3 _direction;
    private DroneSimpleScr _target;
    private DroneState _currentState;

    private void Update()
    {
        switch (_currentState)
        {
            case DroneState.Wander:
                if (NeedsDestination())
                {
                    GetDestinaton();
                }

                transform.rotation = _desiredRotation;
                transform.Translate(Vector3.forward * Time.deltaTime * 5f);

                var rayColor = IsPathBlocked() ? Color.red : Color.green;
                Debug.DrawRay(transform.position, _direction * _rayDisntace, rayColor);

                while (IsPathBlocked())
                {
                    Debug.Log("PathBlocked");
                    GetDestinaton();
                }
                var targetToAggro = CheckForAggro();
                if (targetToAggro != null)
                {
                    _target = targetToAggro.GetComponent<DroneSimpleScr>();
                    _currentState = DroneState.Shase;
                }

                break;
            case DroneState.Shase:
                if (_target == null)
                {
                    _currentState = DroneState.Wander;
                    return;
                }

                transform.LookAt(_target.transform);
                transform.Translate(Vector3.forward * Time.deltaTime * 5f);

                if (Vector3.Distance(transform.position, _target.transform.position) < _attackRange)
                {
                    _currentState = DroneState.Attack;
                }

                break;
            case DroneState.Attack:

                if (_target != null)
                {
                    Destroy(_target.gameObject);
                }
                _currentState = DroneState.Wander;
                break;
            default:
                break;
        }
    }

    private bool IsPathBlocked()
    {
        Ray ray = new Ray(transform.position, _direction);
        var hitSomething = Physics.RaycastAll(ray, _rayDisntace, _layerMask);
        return hitSomething.Any();
    }

    private void GetDestinaton()
    {
        Vector3 testPostion = (transform.position + (transform.forward * 4f)) +
                               new Vector3(Random.Range(-4.5f, 4.5f), 0f, Random.Range(-4.5f, 4.5f));

        _destination = new Vector3(testPostion.x, 1f, testPostion.z);
        _direction = Vector3.Normalize(_destination - transform.position);
        _direction = new Vector3(_direction.x, 0f, _direction.z);
        _desiredRotation = Quaternion.LookRotation(_direction);
    }

    private bool NeedsDestination()
    {
        if (_destination == Vector3.zero)
        {
            return true;
        }
        var distance = Vector3.Distance(transform.position, _destination);
        if (distance <= _stoppingDistance)
        {
            return true;
        }

        return false;
    }

    Quaternion startingAngle = Quaternion.AngleAxis(-60, Vector3.up);
    Quaternion stepAngle = Quaternion.AngleAxis(5, Vector3.up);

    private Transform CheckForAggro()
    {
        float aggroRadius = 5f;
        RaycastHit hit;
        var angle = transform.rotation * startingAngle;
        var direction = angle * Vector3.forward;
        var pos = transform.position;
        for (int i = 0; i < 24; i++)
        {
            if (Physics.Raycast(pos, direction, out hit, aggroRadius))
            {
                var drone = hit.collider.GetComponent<DroneSimpleScr>();
                if (drone != null && drone.Team != gameObject.GetComponent<DroneSimpleScr>().Team)
                {
                    Debug.DrawRay(pos, direction * hit.distance, Color.red);
                    return drone.transform;
                }
                else
                {
                    Debug.DrawRay(pos, direction * hit.distance, Color.yellow);
                }
            }
            else
            {
                Debug.DrawRay(pos, direction * aggroRadius, Color.white);
            }
            direction = stepAngle * direction;
        }
        return null;
    }
}
