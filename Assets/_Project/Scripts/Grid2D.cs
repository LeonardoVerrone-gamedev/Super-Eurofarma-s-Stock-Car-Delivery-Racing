using UnityEngine;
using System.Collections.Generic;

public class Grid2D : MonoBehaviour
{
    public Transform player;
    public LayerMask obstacleLayer;
    public Vector2 gridWorldSize = new Vector2(40, 40);
    public float nodeRadius = 0.5f;
    public float updateDistance = 2f;

    Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;
    Vector2 lastCenter;

    static readonly int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
    static readonly int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        RebuildGrid();
    }

    void LateUpdate()
    {
        if (Vector2.Distance(player.position, lastCenter) > updateDistance)
        {
            lastCenter = player.position;

            Vector2 offset = player.up * (gridWorldSize.y * 0.25f);
            transform.position = (Vector2)player.position + offset;

            RebuildGrid();
        }
    }

    void RebuildGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        Vector2 bottomLeft = (Vector2)transform.position
            - Vector2.right * gridWorldSize.x / 2
            - Vector2.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = bottomLeft
                    + Vector2.right * (x * nodeDiameter + nodeRadius)
                    + Vector2.up * (y * nodeDiameter + nodeRadius);

                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius, obstacleLayer);

                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public Node NodeFromWorld(Vector2 pos)
    {
        Vector2 local = pos - (Vector2)transform.position;

        float px = (local.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float py = (local.y + gridWorldSize.y / 2) / gridWorldSize.y;

        int x = Mathf.FloorToInt(px * gridSizeX);
        int y = Mathf.FloorToInt(py * gridSizeY);

        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
            return null;

        return grid[x, y];
    }

    public void GetNeighbors(Node node, List<Node> result)
    {
        result.Clear();

        for (int i = 0; i < 8; i++)
        {
            int nx = node.x + dx[i];
            int ny = node.y + dy[i];

            if (nx >= 0 && nx < gridSizeX && ny >= 0 && ny < gridSizeY)
                result.Add(grid[nx, ny]);
        }
    }
}