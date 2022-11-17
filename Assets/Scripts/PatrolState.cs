using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IState
{
    private StateMachine _sm;
    private Enemy _enemy;
    private List<Node> _backPath = new List<Node>();
    private int _currentReturnNode = 0;
    private List<Transform> _visibleNodes = new List<Transform>();

    public PatrolState(Enemy enemy, StateMachine sm)
    {
        _sm = sm;
        _enemy = enemy;
    }

    public void ManualUpdate()
    {
        if (_enemy.Target != null)
        {
            _backPath.Clear();
            _sm.ChangeState("ChaseState");
            return;
        }

        _enemy.Target = _enemy.FOV(_enemy.TargetLayer);

        CheckVisibleNodes();

        if (_visibleNodes.Contains(_enemy.wayPoints[_enemy.CurrentWayPoint]))
        {
            Patrol();
        }
        else
        {
            MoveToNodes();
        }

        if (_backPath.Count == 0)
        {
            _enemy.Move(_enemy.wayPoints[_enemy.CurrentWayPoint].transform.position);
        }
        else
        {
            _enemy.Move(_backPath[_currentReturnNode].transform.position);
        }
    }


    private void Patrol()
    {
        _backPath.Clear();

        _enemy.Move(_enemy.wayPoints[_enemy.CurrentWayPoint].transform.position);

        Vector3 pointDistance = _enemy.wayPoints[_enemy.CurrentWayPoint].transform.position - _enemy.transform.position;

        if (pointDistance.magnitude < _enemy.EnemyProperties.stoppingDistance)
        {
            _enemy.CurrentWayPoint++;
            if (_enemy.CurrentWayPoint > _enemy.wayPoints.Count - 1)
                _enemy.CurrentWayPoint = 0;
        }
    }

    private void CheckVisibleNodes()
    {
        _visibleNodes.Clear();

        Collider[] nodesInViewRadius = Physics.OverlapSphere(_enemy.transform.position, _enemy.ViewRadius, _enemy.NodeLayer);

        foreach (var item in nodesInViewRadius)
        {
            GameObject node = item.gameObject;

            Vector3 dirToTarget = node.transform.position - _enemy.transform.position;

            if (Vector3.Angle(_enemy.transform.forward, dirToTarget) < _enemy.AngleRadius / 2)
            {
                if (!Physics.Raycast(_enemy.transform.position, dirToTarget, dirToTarget.magnitude,
                    _enemy.WallLayer))
                    _visibleNodes.Add(node.transform);
            }
        }
    }

    private void MoveToNodes()
    {
        if (_backPath.Count == 0)
        {
            _backPath = _enemy.GetPath(_enemy.LastGoalNode, _enemy.wayPoints[0].gameObject.GetComponent<Node>());
            _backPath.Reverse();

            _currentReturnNode = 0;
            _enemy.CurrentWayPoint = 0;

            if (_backPath.Count == 0)
            {
                Patrol();
            }
        }

        _enemy.Move(_backPath[_currentReturnNode].transform.position);

        Vector3 pointDistance = _backPath[_currentReturnNode].transform.position - _enemy.transform.position;

        if (pointDistance.magnitude < _enemy.EnemyProperties.stoppingDistance)
            _currentReturnNode++;
    }
}