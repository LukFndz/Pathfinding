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
        if (_onGoalNode) // SI LLEGÓ AL ULTIMO NODO, VA HACIA LA ULTIMA POS DEL PLAYER
        {
            if (_enemy.name == "Blue")
                Debug.Log(_onGoalNode);

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

                Node goalNode = GetNerbyTargetNode(); // EL NODO FINAL SERÁ EL MAS CERCANO AL TARGET

                if (goalNode == null)
                {
                    if (_enemy.name == "Blue")
                        Debug.Log("ESTABA NULO");
                    _onGoalNode = true;
                    return;
                }

                _enemy.ChasePath = _enemy.GetPath(_enemy.GetNerbyNode(), goalNode); //CONSTRUYE EL CAMINO DESDE EL NODO MAS CERCANO HASTA EL MAS CERCANO AL PLAYER
                _enemy.ChasePath.Reverse(); // INVIERTE LA LISTA (EL CAMINO)

                _currentChaseNode = 0;

                if (_enemy.ChasePath.Count == 0)
                {
                    _onGoalNode = true;
                    return;
                }
            }

            _enemy.Move(_enemy.ChasePath[_currentChaseNode].transform.position); // SE MUEVE AL SIGUIENTE NODO

            Vector3 pointDistance = _enemy.ChasePath[_currentChaseNode].transform.position - _enemy.transform.position;

            if (pointDistance.magnitude < _enemy.EnemyProperties.stoppingDistance) // CHECKEA SI LLEGÓ
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

        float distance = float.MaxValue; // SETEA LA DISTANCIA AL MAXIMO COMO PRIMER VALOR

        List<Node> allNodes = GameObject.FindObjectsOfType<Node>().ToList();

        foreach (var node in allNodes) // CHECK TODOS LOS NODOS A VER CUAL ESTÁ MAS CERCA DEL TARGET 
        {
            GameObject target = node.gameObject;

            Vector3 dirToTarget = target.transform.position - _enemy.Target.transform.position;

            if (!Physics.Raycast(_enemy.Target.transform.position, dirToTarget, dirToTarget.magnitude, _enemy.WallLayer)) //CHECK QUE LOS NODOS ESTEN AL ALCANCE DEL PLAYER (NO ATRAVIESEN PAREDES)
                if (dirToTarget.magnitude < distance)
                {
                    distance = dirToTarget.magnitude;
                    nerbyNode = node.gameObject;
                }
        }

        return nerbyNode.GetComponent<Node>();
    }
}
