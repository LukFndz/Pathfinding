using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChaseState : IState
{
    private StateMachine _sm;
    private readonly Enemy _enemy;
    private int _currentChaseNode = 0;
    private bool _targetIsVisible;
    private bool _lastNodeReached;

    public ChaseState(Enemy enemy, StateMachine sm)
    {
        _sm = sm;
        _enemy = enemy;
    }

    public void OnUpdate()
    {
        GameObject g = _enemy.ApplyFOV(_enemy.TargetLayer); // SI SE VE EL PLAYER, RETORNA, SINO, ES NULL
        _targetIsVisible = g;

        if (_targetIsVisible) // SI VE AL PLAYER
        {
            _enemy.LastTargetPosition = g.transform.position; // SETEA CONSTANTEMENTE LA ULTIMA POS DEL PLAYER
            _enemy.AlertEnemies(); // ALERTA ENEMIGOS
            ChaseTarget(); // CHASE AL PLAYER
        }
        else
        {
            MoveToLastPos();
        }
    }

    private void ChaseTarget() // CHASE AL PLAYER
    {
        if (_enemy.Target == null)
            return;

        _enemy.Move(_enemy.Target.transform.position);
    }

    private void MoveToLastPos()
    {
        if (_lastNodeReached) // SI LLEGÓ AL ULTIMO NODO, VA HACIA LA ULTIMA POS DEL PLAYER
        {
            _enemy.Move(_enemy.LastTargetPosition);

            Vector3 pointDistance = _enemy.LastTargetPosition - _enemy.transform.position;

            if (pointDistance.magnitude < _enemy.EnemyProperties.stoppingDistance) // SI NO LO VE, CAMBIA DE ESTADO
            {
                _enemy.Target = null;
                _sm.ChangeState("PatrolState");
            }
        }
        else
        {
            if (_enemy.ChasePath.Count == 0) // CHECK SI YA EXISTE CAMINO
            {
                Node goalNode = GetNerbyTargetNode(); // EL NODO FINAL SERÁ EL MAS CERCANO AL TARGET

                if (goalNode == null)
                {
                    _lastNodeReached = true;
                    return;
                }

                _enemy.ChasePath = _enemy.ConstructPath(_enemy.GetNerbyNode(), goalNode); //CONSTRUYE EL CAMINO DESDE EL NODO MAS CERCANO HASTA EL MAS CERCANO AL PLAYER
                _enemy.ChasePath.Reverse(); // INVIERTE LA LISTA (EL CAMINO)

                _currentChaseNode = 0;

                if (_enemy.ChasePath.Count == 0)
                {
                    _lastNodeReached = true;
                    return;
                }
            }

            _enemy.Move(_enemy.ChasePath[_currentChaseNode].transform.position); // SE MUEVE AL SIGUIENTE NODO

            Vector3 pointDistance = _enemy.ChasePath[_currentChaseNode].transform.position - _enemy.transform.position;

            if (pointDistance.magnitude < _enemy.EnemyProperties.stoppingDistance) // CHECKEA SI LLEGÓ
            {
                _currentChaseNode++; // SIGUIENTE NODO
                if (_currentChaseNode > _enemy.ChasePath.Count - 1) // CHECK SI LLEGO AL ULTIMO
                    _lastNodeReached = true;
            }
        }
    }

    private Node GetNerbyTargetNode()
    {
        if (_enemy.Target == null)
            return null;

        GameObject nerbyNode = null;

        List<Node> allNodes = GameObject.FindObjectsOfType<Node>().ToList();

        float distance = float.MaxValue; // SETEA LA DISTANCIA AL MAXIMO COMO PRIMER VALOR

        foreach (var item in allNodes) // CHECK TODOS LOS NODOS A VER CUAL ESTÁ MAS CERCA DEL TARGET
        {
            Vector3 nodeDistance = item.transform.position - _enemy.Target.transform.position;

            if (nodeDistance.magnitude < distance)
            {
                distance = nodeDistance.magnitude;
                nerbyNode = item.gameObject;
            }
        }

        return nerbyNode.GetComponent<Node>();
    }
}
