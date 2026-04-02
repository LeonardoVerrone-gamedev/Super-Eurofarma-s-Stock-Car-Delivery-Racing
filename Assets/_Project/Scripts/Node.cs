using UnityEngine;
using System.Collections.Generic;

public class Node
{
    public bool walkable;
    public Vector2 worldPos;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public Node parent;

    public int heapIndex;
    public bool inOpenSet;

    public int searchId;
    public int closedId;

    public int fCost => gCost + hCost;

    public Node(bool walkable, Vector2 pos, int x, int y)
    {
        this.walkable = walkable;
        this.worldPos = pos;
        this.x = x;
        this.y = y;
    }
}

public class MinHeap
{
    List<Node> items = new();

    public int Count => items.Count;

    public void Add(Node item)
    {
        item.heapIndex = items.Count;
        items.Add(item);
        SortUp(item);
    }

    public Node RemoveFirst()
    {
        Node first = items[0];
        int lastIndex = items.Count - 1;

        if (lastIndex == 0)
        {
            items.RemoveAt(0);
            return first;
        }

        items[0] = items[lastIndex];
        items[0].heapIndex = 0;

        items.RemoveAt(lastIndex);

        SortDown(items[0]);

        return first;
    }

    public void UpdateItem(Node item)
    {
        SortUp(item);
    }

    void SortUp(Node node)
    {
        while (node.heapIndex > 0)
        {
            int parentIndex = (node.heapIndex - 1) / 2;
            Node parent = items[parentIndex];

            if (Compare(node, parent) < 0)
                Swap(node, parent);
            else
                break;
        }
    }

    void SortDown(Node node)
    {
        while (true)
        {
            int left = node.heapIndex * 2 + 1;
            int right = node.heapIndex * 2 + 2;

            if (left >= items.Count)
                break;

            int smallest = left;

            if (right < items.Count && Compare(items[right], items[left]) < 0)
                smallest = right;

            if (Compare(items[smallest], node) < 0)
                Swap(node, items[smallest]);
            else
                break;
        }
    }

    void Swap(Node a, Node b)
    {
        items[a.heapIndex] = b;
        items[b.heapIndex] = a;

        int temp = a.heapIndex;
        a.heapIndex = b.heapIndex;
        b.heapIndex = temp;
    }

    int Compare(Node a, Node b)
    {
        int f = a.fCost.CompareTo(b.fCost);
        return f == 0 ? a.hCost.CompareTo(b.hCost) : f;
    }
}