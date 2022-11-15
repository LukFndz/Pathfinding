using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChaseState : IState
{
    private StateMachine _sm;
    private Enemy _enemy;
    private int _currentChaseNode = 0;
    private bool _seeTarget;
    private bool _onGoalNode;

    public ChaseState(Enemy enemy, StateMachine sm)
    {
        _sm = sm;
        _enemy = enemy;
    }

    public void ManualUpdate()
    {
        GameObject g = _enemy.FOV(_enemy.TargetLayer); // SI SE VE EL PLAYER, RETORNA, SINO, ES NULL
        _seeTarget = g;

        if (_seeTarget) // SI VE AL PLAYER
        {
            _enemy.LastTargetPosition = g.transform.position; // SETEA CONSTANTEMENTE LA ULTIMA POS DEL PLAYER
            _enemy.Alert(); // ALERTA ENEMIGOS
            Chase(); // CHASE AL PLAYER
        }
        else
        {
            GoToLastPos();
        }
    }

    private void Chase() // CHASE AL PLAYER
    {
        if (_enemy.Target == null)
            return;

        _enemy.Move(_enemy.Target.transform.position);
    }

    private void GoToLastPos()
    {
        if (_onGoalNode) // SI LLEG� AL ULTIMO NODO, VA HACIA LA ULTIMA POS DEL PLAYER
        {
            _enemy.ChasePath.Clear();

            _enemy.Move(_enemy.LastTargetPosition);

            Vector3 pointDistance = _enemy.LastTargetPosition - _enemy.transform.position;

            if (pointDistance.magnitude < _enemy.EnemyProperties.stoppingDistance) // SI NO LO VE, CAMBIA DE ESTADO
            {
                _onGoalNode = false;
                _enemy.Target = null;
                _sm.ChangeState("PatrolState");
            }
        }
        else
        {
            if (_enemy.ChasePath.Count == 0) // CHECK SI YA EXISTE CAMINO
            {
                Node goalNode = GetNerbyTargetNode(); // EL NODO FINAL SER� EL MAS CERCANO AL TARGET

                if (goalNode == null)
                {
                    _onGoalNode = true;
                    return;
                }
                _enemy.ChasePath = _enemy.GetPath(_enemy.GetNerbyNode(), goalNode); //CONSTRUYE EL CAMINO DESDE EL NODO MAS CERCANO HASTA EL MAS CERCANO AL PLAYER
                _enemy.ChasePath.Reverse(); // INVIERTE LA LISTA (EL CAMINO)
                if(_enemy.ChasePath.Count > 1)
                    _enemy.ChasePath.RemoveAt(0);
                _currentChaseNode = 0;

                if (_enemy.ChasePath.Count == 0)
                {
                    _onGoalNode = true;
                    return;
                }
            }

            _enemy.Move(_enemy.ChasePath[_currentChaseNode].transform.position); // SE MUEVE AL SIGUIENTE NODO

            Vector3 pointDistance = _enemy.ChasePath[_currentChaseNode].transform.position - _enemy.transform.position;

            if (pointDistance.magnitude < _enemy.EnemyProperties.stoppingDistance) // CHECKEA SI LLEG�
            {
                _currentChaseNode++; // SIGUIENTE NODO
                if (_currentChaseNode > _enemy.ChasePath.Count - 1) // CHECK SI LLEGO AL ULTIMO
                    _onGoalNode = true;
            }
        }
    }

    private Node GetNerbyTargetNode()
    {
        if (_enemy.Target == null)
            return null;

        GameObject nerbyNode = null;

        List<Node> allNodes = GameObject.FindObjectsOfType<Node>().ToList();

        float distance = float.MaxValue;

        foreach (var item in allNodes)
        {
            Vector3 nodeDistance = item.transform.position - _enemy.Target.transform.position;

            if (nodeDistance.magnitude < distance)
            {
                if (!Physics.Raycast(_enemy.Target.transform.position, nodeDistance, nodeDistance.magnitude, _enemy.WallLayer))
                {
                    distance = nodeDistance.magnitude;
                    nerbyNode = item.gameObject;
                }
            }
        }
        return nerbyNode.GetComponent<Node>();
    }
}

