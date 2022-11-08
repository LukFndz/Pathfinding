using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue
{
    private Dictionary<Node, float> _allElems = new Dictionary<Node, float>();

    public void Enqueue(Node node, float cost)
    {
        _allElems.Add(node, cost);
    }

    public Node Dequeue()
    {
        if (Count() == 0)
            return null;

        Node n = null;

        foreach (var item in _allElems)
        {
            if (n == null)
                n = item.Key;

            if (item.Value < _allElems[n])
                n = item.Key;
        }

        _allElems.Remove(n);

        return n;
    }

    public int Count()
    {
        return _allElems.Count;
    }
}
