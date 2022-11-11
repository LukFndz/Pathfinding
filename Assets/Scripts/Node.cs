using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> neighbors;
    [SerializeField] private int _cost = 1;

    public int Cost { get => _cost; set => _cost = value; }
}
