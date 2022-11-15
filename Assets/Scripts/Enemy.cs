using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float _catchRadius;

    [Header("LayerMask")]
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private LayerMask _nodeLayer;
    [SerializeField] private LayerMask _wallLayer;


    private GameObject _target;

    private int _currentWayPoint = 0;

    [SerializeField]private List<Node> _chasePath = new List<Node>();

    private Vector3 _lastTargetPosition;

    private StateMachine _sm;
    private List<Enemy> _enemies;


    private Vector3 dirToTarget;
    private Ray ray;
    private RaycastHit hit;


    public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }
    public LayerMask NodeLayer { get => _nodeLayer; set => _nodeLayer = value; }
    public LayerMask WallLayer { get => _wallLayer; set => _wallLayer = value; }
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
        _enemies = FindObjectsOfType<Enemy>().Where(x => x != this).ToList(); // BUSCA TODOS LOS ENEMIGOS MEDIANTE LINQ
    }

    private void Update()
    {
        _sm.ManualUpdate();
        Collider[] playerCheck = Physics.OverlapSphere(transform.position, _catchRadius, _targetLayer);

        if(playerCheck.Count() > 0)
        {
            SceneManager.LoadScene(0);
        }
    }
    public GameObject FOV(LayerMask targetMask) // FIELD OF VIEW / LINE OF SIGHT
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, _viewRadius, targetMask);
        int layerMask = 1 << 9;

        foreach (var item in targetsInViewRadius)
        {
            GameObject target = item.gameObject;

            dirToTarget = target.transform.position - transform.position;

            if (Vector3.Angle(transform.forward, dirToTarget) < _angleRadius / 2)
            {
                if (!Physics.Raycast(transform.position, dirToTarget, dirToTarget.magnitude, layerMask))
                    return target;
            }
        }

        return null;
    }
    public List<Node> GetPath(Node startingNode, Node goalNode) // CONSIGUE EL CAMINO OPTIMO ENTRE LOS DOS NODOS ENVIADOS
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
                path.Add(startingNode);
                return path;
            }

            foreach (var neighbor in current.neighbors)
            {
                float newCost = costSoFar[current] + neighbor.Cost;
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    float priority = newCost + GetDistance(neighbor.transform.position, goalNode);
                    frontier.Enqueue(neighbor, priority);
                    cameFrom[neighbor] = current;
                }
            }
        }
        return null;
    }

    public void Alert() // ALERTA A LOS ENEMIGOS DE QUE VIÓ AL PLAYER
    {
        foreach (Enemy e in _enemies)
        {
            e._chasePath.Clear();
            e._target = _target;
            e._lastTargetPosition = _target.transform.position;
        }
    }

    private float GetDistance(Vector3 vector, Node node) // CONSIGUE LA DISTANCIA ENTRE EL NODO Y LA POS QUE LE PASE
    {
        return Mathf.Abs((node.transform.position - vector).magnitude);
    }

    public void Move(Vector3 newPos) // MOVIMIENTO DEL ENEMIGO
    {
        float step = _enemyProperties.speed * Time.deltaTime;

        newPos.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, newPos, step);

        Rotate(newPos);
    }

    private void Rotate(Vector3 newPos) // ROTACION DEL ENEMIGO
    {
        newPos.y = transform.position.y;
        transform.LookAt(newPos);
    }

    public Node GetNerbyNode() // CONSIGUE EL NODO MAS CERCA
    {
        GameObject nerbyNode = null;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, _viewRadius, _nodeLayer);
        float distance = float.MaxValue;

        foreach (var item in targetsInViewRadius)
        {
            Vector3 nodeDistance = item.transform.position - transform.position;

            if (nodeDistance.magnitude < distance)
            {
                if (!Physics.Raycast(transform.position, nodeDistance, nodeDistance.magnitude, _wallLayer))
                {
                    distance = nodeDistance.magnitude;
                    nerbyNode = item.gameObject;
                }
            }
        }

        return nerbyNode.GetComponent<Node>();
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
