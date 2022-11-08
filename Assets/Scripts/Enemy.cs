using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public struct EnemyProperties
{
    public float speed;
    public float stoppingDistance;
}

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyProperties _enemyProperties;

    public List<Transform> wayPoints;

    [Header("FOV")]
    [SerializeField] private float _viewRadius;
    [SerializeField] private float _angleRadius;

    [Header("LayerMasks")]
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private LayerMask _nodeLayer;
    [SerializeField] private LayerMask _obstacleLayer;


    private GameObject _target;

    private int _currentWayPoint = 0;

    private List<Node> _chasePath = new List<Node>();

    private Vector3 _lastTargetPosition;

    private StateMachine _sm;
    private List<Enemy> _enemies;

    public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }
    public LayerMask NodeLayer { get => _nodeLayer; set => _nodeLayer = value; }
    public LayerMask ObstacleLayer { get => _obstacleLayer; set => _obstacleLayer = value; }
    public Vector3 LastTargetPosition { get => _lastTargetPosition; set => _lastTargetPosition = value; }
    public GameObject Target { get => _target; set => _target = value; }
    public EnemyProperties EnemyProperties { get => _enemyProperties; set => _enemyProperties = value; }
    public List<Node> ChasePath { get => _chasePath; set => _chasePath = value; }
    public int CurrentWayPoint { get => _currentWayPoint; set => _currentWayPoint = value; }
    public float ViewRadius { get => _viewRadius; set => _viewRadius = value; }
    public float AngleRadius { get => _angleRadius; set => _angleRadius = value; }

    private void Awake()
    {
        _sm = GetComponent<StateMachine>();
        _sm.AddState("PatrolState", new PatrolState(this, _sm));
        _sm.AddState("ChaseState", new ChaseState(this, _sm));
        _sm.ChangeState("PatrolState");
    }

    private void Start()
    {
        _enemies = GameObject.FindObjectsOfType<Enemy>().Where(x => x != this).ToList();
    }

    private void Update()
    {
        _sm.OnUpdate();
    }

    public List<Node> ConstructPath(Node startingNode, Node goalNode)
    {
        PriorityQueue frontier = new PriorityQueue();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> costSoFar = new Dictionary<Node, float>();

        frontier.Enqueue(startingNode, 0);
        cameFrom.Add(startingNode, null);
        costSoFar.Add(startingNode, 0);

        int frontierCount = frontier.Count();

        while (frontierCount != 0)
        {
            Node current = frontier.Dequeue();

            if (current == goalNode)
            {
                List<Node> path = new List<Node>();
                while (current != startingNode)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                return path;
            }

            foreach (var next in current.neighbors)
            {
                float newCost = costSoFar[current] + next.Cost;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + Heuristic(next.transform.position, goalNode);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }
        return null;
    }

    public void AlertEnemies()
    {
        foreach (Enemy enemy in _enemies)
        {
            enemy._chasePath.Clear();
            enemy._target = _target;
            enemy._lastTargetPosition = _target.transform.position;
        }
    }

    public GameObject ApplyFOV(LayerMask targetMask)
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, _viewRadius, targetMask);

        foreach (var item in targetsInViewRadius)
        {
            GameObject target = item.gameObject;

            Vector3 dirToTarget = target.transform.position - transform.position;

            if (Vector3.Angle(transform.forward, dirToTarget) < _angleRadius / 2)
            {
                if (!Physics.Raycast(transform.position, dirToTarget, dirToTarget.magnitude, _obstacleLayer))
                    return target;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the enemy's closest node 
    /// </summary>
    /// <returns></returns>
    public Node GetNerbyNode()
    {
        GameObject nerbyNode = null;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, _viewRadius, _nodeLayer);

        float distance = 999f;

        foreach (var item in targetsInViewRadius)
        {
            Vector3 nodeDistance = item.transform.position - transform.position;

            if (nodeDistance.magnitude < distance)
            {
                distance = nodeDistance.magnitude;
                nerbyNode = item.gameObject;
            }
        }

        return nerbyNode.GetComponent<Node>();
    }

    public void Move(Vector3 newPos)
    {
        float step = _enemyProperties.speed * Time.deltaTime;

        newPos.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, newPos, step);

        Rotate(newPos);
    }

    private void Rotate(Vector3 newPos)
    {
        newPos.y = transform.position.y;
        transform.LookAt(newPos);
    }

    private float Heuristic(Vector3 pos, Node goalNode)
    {
        return Mathf.Abs((goalNode.transform.position - pos).magnitude);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float totalFOV = _angleRadius;
        float rayRange = _viewRadius;
        float halfFOV = totalFOV / 2.0f;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;
        Gizmos.DrawRay(transform.position, leftRayDirection * rayRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * rayRange);
    }
}
